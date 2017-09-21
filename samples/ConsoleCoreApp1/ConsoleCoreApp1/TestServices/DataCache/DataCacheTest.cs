using Glory.Services.Core.Config;
using Glory.Services.Core.DataCache;
using Glory.Services.Core.DataCache.Parameters;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleCoreApp1.TestServices.DataCache
{
    public class DataCacheTest
    {
        private readonly IDataCacheManager _dataCacheManager;
        private readonly ILogger<DataCacheTest> _logger;

        public DataCacheTest(IDataCacheManager dataCacheManager
            , ILogger<DataCacheTest> logger)
        {
            _dataCacheManager = dataCacheManager;
            _logger = logger;
        }
        public void Test()
        {
            var nval = _dataCacheManager.GetCachedData(new CacheItemArgs("testcore", 5), (cargs) =>
            {
                return (long)5;
            });
            var nval2 = _dataCacheManager.IncrementValue("testcore", () =>
           {
               return 10;
           });

            var strResult = $"IncrementValue-Low:{nval}->{nval2}";

            _logger.LogInformation(strResult);

            nval2 = _dataCacheManager.GetCachedDataLocalDistributed("testcoredistributed", (cargs) =>
            {
                return 10;
            }, 1);

            strResult = $"DataLocalDistributed:{nval}->{nval2}";

            _logger.LogInformation(strResult);
        }
    }
}
