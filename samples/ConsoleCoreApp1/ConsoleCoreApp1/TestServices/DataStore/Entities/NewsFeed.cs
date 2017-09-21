using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleCoreApp1.TestServices.DataStore.Entities
{
    public class NewsFeed
    {
        public long Id { get; set; }
        public string Message { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
    }
}
