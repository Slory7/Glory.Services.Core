using ConsoleCoreApp1.TestServices.DataStore.Entities;
using Glory.Services.Core.Config;
using Glory.Services.Core.DataCache;
using Glory.Services.Core.DataStore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConsoleCoreApp1.TestServices.DataStore
{
    public class DataStoreTest
    {
        private readonly ILogger<DataStoreTest> _logger;
        private readonly IDataStoreManager _dataStoreManager;
        private readonly IDataCacheManager _dataCacheManager;

        public DataStoreTest(
            IDataStoreManager dataStoreManager,
            IDataCacheManager dataCacheManager,
            ILogger<DataStoreTest> logger)
        {
            _dataStoreManager = dataStoreManager;
            _dataCacheManager = dataCacheManager;
            _logger = logger;
        }

        public async Task Test()
        {
            await TestStorage();

            await TestStage();
        }

        private async Task TestStorage()
        {
            _logger.LogTrace("TestStorage: start");

            var id = _dataCacheManager.IncrementValue("CustomerId", () =>
            {
                return _dataStoreManager.Queryable<Customer>().OrderByDescending(c => c.Id).FirstOrDefault()?.Id ?? 0;
            });
            var customer = new Customer()
            {
                Id = id,
                Name = "Andy",
                CreatedDate = DateTime.Now
            };

            await _dataStoreManager.Insert<Customer>(customer);

            var lastcustomer = _dataStoreManager.Queryable<Customer>()
                .Where(c => c.CreatedDate >= DateTime.Today).OrderByDescending(c => c.Id).First();

            _logger.LogInformation($"last one:id: {lastcustomer.Id},name:{lastcustomer.Name},create date:{lastcustomer.CreatedDate.LocalDateTime}");

            var firstcustomer = _dataStoreManager.Queryable<Customer>().First();

            await _dataStoreManager.Remove<Customer>((c) => c.Id == 5);

            _logger.LogInformation($"remove first one:id: {firstcustomer.Id},name:{firstcustomer.Name},create date:{firstcustomer.CreatedDate.LocalDateTime}");
        }

        private async Task TestStage()
        {
            var id = _dataCacheManager.IncrementValue("NewsFeed", () =>
           {
               return _dataStoreManager.Queryable<NewsFeed>().OrderByDescending(c => c.Id).FirstOrDefault()?.Id ?? 0;
           });

            var now = DateTime.Now;

            var newsfeed = new NewsFeed()
            {
                Id = id,
                Message = "New Message",
                CreatedDate = now
            };

            await _dataStoreManager.InsertStageData<NewsFeed>(newsfeed);      

            _logger.LogInformation($"insert stage one:id: {newsfeed.Id},msg:{newsfeed.Message},create date:{newsfeed.CreatedDate.LocalDateTime}");

            var lastFetchDate = _dataCacheManager.GetCache<DateTime?>("News Fetch Date");

            int lastOnStageMinutes = lastFetchDate == null ? -1 : (int)now.Subtract(lastFetchDate.Value).TotalMinutes;

            var lastestQuery = _dataStoreManager.StageQueryable<NewsFeed>(lastOnStageMinutes);
            if (lastFetchDate != null)
                lastestQuery = lastestQuery.Where(c => c.CreatedDate >= lastFetchDate);

            var nNewCount = lastestQuery.Count();
            _logger.LogInformation($"lasted count :{nNewCount}");

            if (nNewCount > 0)
            {
                var lastone = lastestQuery.OrderByDescending(c => c.Id).First();
                _logger.LogInformation($"last one:id: {lastone.Id},msg:{lastone.Message},create date:{lastone.CreatedDate.LocalDateTime}");
            }
            _dataCacheManager.SetCache("News Fetch Date", now);
        }
    }
}
