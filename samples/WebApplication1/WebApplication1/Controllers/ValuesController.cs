using Glory.Services.Core.DataCache;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace WebApplication1.Controllers
{
    public class ValuesController : ApiController
    {
        private readonly ILogger<ValuesController> _logger;
        private readonly IDataCacheManager _cacheManager;

        public ValuesController(
            IDataCacheManager cacheManager
            , ILogger<ValuesController> logger)
        {
            _logger = logger;
            _cacheManager = cacheManager;
        }

        // GET api/values
        public IEnumerable<string> Get()
        {
            var val = _cacheManager.IncrementValue("WebApplication1_Test",
                () =>
            {
                return 100;
            }
            , expiredMinutes: 10);

            _logger.LogInformation($"ValuesController:Get:Values:{val}");

            return new string[] { "value1", "value2", val.ToString() };
        }

        // GET api/values/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}
