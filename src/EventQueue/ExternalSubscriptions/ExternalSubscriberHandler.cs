using Glory.Services.Core.Config;
using Glory.Services.Core.DataCache;
using Glory.Services.Core.EventQueue.EventBus.Abstractions;
using Glory.Services.Core.Internal;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Glory.Services.Core.EventQueue.ExternalSubscriptions
{
    public class ExternalSubscriberHandler : IDynamicIntegrationEventHandler
    {
        private IExternalSubscriptionsManager _IExternalSubscriptionsManager;
        private IDataCacheManager _IDataCacheManager;
        private ILogger<ExternalSubscriberHandler> _logger;

        public ExternalSubscriberHandler(IExternalSubscriptionsManager externalSubscriptionsManager
            , IDataCacheManager cacheManger
            , ILogger<ExternalSubscriberHandler> logger
            )
        {
            _IExternalSubscriptionsManager = externalSubscriptionsManager;
            _IDataCacheManager = cacheManger;
            _logger = logger;
        }

        public Task Handle(string eventName, dynamic eventData)
        {
            var publishedEvent = _IExternalSubscriptionsManager.GetPublishedEvent(eventName);
            if (publishedEvent.EventThresholdSeconds > 0)
            {
                var cachekey = eventName + "_LastHandleTime";
                var lastHandleTime = _IDataCacheManager.GetCache<DateTime?>(cachekey);
                if (lastHandleTime != null && DateTime.Now.Subtract(lastHandleTime.Value).TotalSeconds < publishedEvent.EventThresholdSeconds)
                    return Task.FromResult(0);

                _IDataCacheManager.SetCache(cachekey, DateTime.Now);
            }

            var subscribers = _IExternalSubscriptionsManager.GetPublicSubscribersByEventName(eventName);

            var resilientHttpClient = new ResilientHttpClient((c) => CreatePolicies(), _logger);
            foreach (var subscriber in subscribers)
            {
                string strNewAddress = subscriber.Address + (subscriber.Address.IndexOf("?")==-1 ? "?" : "&");
                strNewAddress += "eventname=" + eventName;
                if (String.IsNullOrEmpty(subscriber.PrivateKey))
                {
                    resilientHttpClient.PostAsync(strNewAddress, eventData);
                }
                else
                {
                    var message = JsonConvert.SerializeObject(eventData);
                    var encryptMessage = AESCryptor.EncryptStringAES(message, subscriber.PrivateKey);
                    resilientHttpClient.PostAsync(strNewAddress, encryptMessage);
                }
            }
            return Task.FromResult(1);
        }
        private Policy[] CreatePolicies()
            => new Policy[]
            {
                Policy.Handle<HttpRequestException>()
                .WaitAndRetryAsync(
                    // number of retries
                    6,
                    // exponential backofff
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    // on retry
                    (exception, timeSpan, retryCount, context) =>
                    {
                        var msg = $"Retry {retryCount} implemented with Polly's RetryPolicy " +
                            $"of {context.PolicyKey} " +
                            $"at {context.ExecutionKey}, " +
                            $"due to: {exception}.";
                        _logger.LogWarning(msg);
                        _logger.LogDebug(msg);
                    }),
                Policy.Handle<HttpRequestException>()
                .CircuitBreakerAsync(
                   // number of exceptions before breaking circuit
                   5,
                   // time circuit opened before retry
                   TimeSpan.FromMinutes(1),
                   (exception, duration) =>
                   {
                        // on circuit opened
                        _logger.LogTrace("Circuit breaker opened");
                   },
                   () =>
                   {
                        // on circuit closed
                        _logger.LogTrace("Circuit breaker reset");
                   })
            };
    }
}
