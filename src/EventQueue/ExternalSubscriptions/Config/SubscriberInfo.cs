using System;

namespace Glory.Services.Core.EventQueue.ExternalSubscriptions.Config
{
    public class SubscriberInfo
    {
        public string Address
        {
            get;
            set;
        }

        public string Description
        {
            get;
            set;
        }

        public string ID
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public string PrivateKey
        {
            get;
            set;
        }

        public SubscriberInfo()
        {
            this.ID = Guid.NewGuid().ToString();
            this.Name = "";
            this.Description = "";
            this.Address = "";
            this.PrivateKey = Guid.NewGuid().ToString();
        }

        public SubscriberInfo(string subscriberName) : this()
        {
            this.Name = subscriberName;
        }
    }
}