using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleCoreApp1.TestServices.DataStore.Entities
{
    public class Customer
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
    }
}
