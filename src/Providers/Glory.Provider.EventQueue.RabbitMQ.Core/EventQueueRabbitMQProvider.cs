using Glory.Services.Core.EventQueue;
using Glory.Services.Core.EventQueue.EventBus;
using Glory.Services.Core.EventQueue.EventBus.Abstractions;
using Glory.Services.Core.EventQueue.EventBus.Events;
using Glory.Services.Core.EventQueue.Providers;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Glory.Provider.EventQueue.RabbitMQ.Core
{
    public class EventQueueRabbitMQProvider : EventQueueProvider, IDisposable
    {
        #region Constructor

        private readonly ILogger<EventQueueRabbitMQProvider> _logger;
        private readonly IEventBusSubscriptionsManager _subsManager;

        private IRabbitMQPersistentConnection _persistentConnection;
        private IModel _consumerChannel;
        private string _queueName;
        public EventQueueRabbitMQProvider(ILogger<EventQueueRabbitMQProvider> logger
            , IEventQueueManager eventQueueManager
            , IEventBusSubscriptionsManager subsManager)
        {
            _logger = logger;
            _subsManager = subsManager;
        }

        #endregion

        #region Public Properties

        public string Host { get; set; }

        public string AppName { get; set; }

        public string QueueName { get; set; } = "";

        public string UserName { get; set; }

        public string Password { get; set; }

        #endregion

        #region Abstract Method Implementation

        public override void Start()
        {
            _subsManager.OnEventRemoved += SubsManager_OnEventRemoved;

            var connlogger = Services.Core.Extensions.GetService<ILogger<RabbitMQPersistentConnection>>();

            var factory = new ConnectionFactory()
            {
                HostName = Host
            };

            if (!string.IsNullOrEmpty(UserName))
            {
                factory.UserName = UserName;
            }

            if (!string.IsNullOrEmpty(Password))
            {
                factory.Password = Password;
            }

            _persistentConnection = new RabbitMQPersistentConnection(factory, connlogger);

            _consumerChannel = CreateConsumerChannel(AppName);
        }

        public override void Publish(IntegrationEvent @event)
        {
            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }

            var policy = RetryPolicy.Handle<BrokerUnreachableException>()
                .Or<SocketException>()
                .WaitAndRetry(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                {
                    _logger.LogWarning(ex.ToString());
                });

            using (var channel = _persistentConnection.CreateModel())
            {
                var eventName = _subsManager.GetEventKey(@event);

                channel.ExchangeDeclare(exchange: AppName,
                                    type: "direct");

                var message = JsonConvert.SerializeObject(@event);
                var body = Encoding.UTF8.GetBytes(message);

                policy.Execute(() =>
                {
                    channel.BasicPublish(exchange: AppName,
                                     routingKey: eventName,
                                     basicProperties: null,
                                     body: body);
                });
            }
        }

        public void Dispose()
        {
            if (_consumerChannel != null)
            {
                _consumerChannel.Dispose();
            }

            _subsManager.Clear();
        }

        public override void Subscribe(string eventName)
        {
            DoInternalSubscription(eventName, AppName);
        }

        public override void Unsubscribe(string eventName)
        {
            //
        }

        #endregion

        #region Private Methods

        private IModel CreateConsumerChannel(string exchangeName)
        {
            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }

            var channel = _persistentConnection.CreateModel();

            channel.ExchangeDeclare(exchange: exchangeName,
                                 type: "direct");

            _queueName = channel.QueueDeclare(QueueName).QueueName;

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                var eventName = ea.RoutingKey;
                var message = Encoding.UTF8.GetString(ea.Body);

                await _subsManager.ProcessEvent(eventName, message);
            };

            channel.BasicConsume(queue: _queueName,
                                 autoAck: false,
                                 consumer: consumer);

            channel.CallbackException += (sender, ea) =>
            {
                _consumerChannel.Dispose();
                _consumerChannel = CreateConsumerChannel(exchangeName);
            };

            return channel;
        }

        private void SubsManager_OnEventRemoved(object sender, string eventName)
        {
            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }

            using (var channel = _persistentConnection.CreateModel())
            {
                channel.QueueUnbind(queue: _queueName,
                    exchange: AppName,
                    routingKey: eventName);

                if (_subsManager.IsEmpty)
                {
                    _consumerChannel.Close();
                }
            }
        }

        private void DoInternalSubscription(string eventName, string exchangeName)
        {
            var containsKey = _subsManager.HasSubscriptionsForEvent(eventName);
            if (!containsKey)
            {
                if (!_persistentConnection.IsConnected)
                {
                    _persistentConnection.TryConnect();
                }

                using (var channel = _persistentConnection.CreateModel())
                {
                    channel.QueueBind(queue: _queueName,
                                      exchange: exchangeName,
                                      routingKey: eventName);
                }
            }
        }      

        #endregion

    }
}
