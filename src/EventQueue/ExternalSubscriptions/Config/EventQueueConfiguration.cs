using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Linq;
using Glory.Services.Core.DataCache;
using Glory.Services.Core.Internal;
using Glory.Services.Core.DataCache.Parameters;
using Glory.Services.Core.EventQueue.ExternalSubscriptions.Config;

namespace Glory.Services.Core.EventQueue.ExternalSubscriptions.Config
{
    internal class EventQueueConfiguration
    {
        const string configFilePath = "config\\EventQueue.config";
        public Dictionary<string, SubscriberInfo> EventQueueSubscribers
        {
            get;
            set;
        }

        public Dictionary<string, PublishedEvent> PublishedEvents
        {
            get;
            set;
        }

        internal EventQueueConfiguration()
        {
            this.PublishedEvents = new Dictionary<string, PublishedEvent>();
            this.EventQueueSubscribers = new Dictionary<string, SubscriberInfo>();
        }

        private void Deserialize(string configXml)
        {
            if (!string.IsNullOrEmpty(configXml))
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(configXml);
                foreach (XmlElement xmlItem in xmlDoc.SelectNodes("/EventQueueConfig/PublishedEvents/Event"))
                {
                    PublishedEvent oPublishedEvent = new PublishedEvent()
                    {
                        EventName = xmlItem.SelectSingleNode("EventName").InnerText,
                        EventThresholdSeconds = int.Parse(xmlItem.SelectSingleNode("EventThresholdSeconds").InnerText),
                        Subscribers = xmlItem.SelectSingleNode("Subscribers").InnerText
                    };
                    this.PublishedEvents.Add(oPublishedEvent.EventName, oPublishedEvent);
                }
                foreach (XmlElement xmlElement in xmlDoc.SelectNodes("/EventQueueConfig/EventQueueSubscribers/Subscriber"))
                {
                    SubscriberInfo oSubscriberInfo = new SubscriberInfo()
                    {
                        ID = xmlElement.SelectSingleNode("ID").InnerText,
                        Name = xmlElement.SelectSingleNode("Name").InnerText,
                        Address = xmlElement.SelectSingleNode("Address").InnerText,
                        Description = xmlElement.SelectSingleNode("Description").InnerText,
                        PrivateKey = xmlElement.SelectSingleNode("PrivateKey").InnerText
                    };
                    this.EventQueueSubscribers.Add(oSubscriberInfo.ID, oSubscriberInfo);
                }
            }
        }

        public static EventQueueConfiguration GetConfig()
        {
            var dataCache = Extensions.GetService<IDataCacheManager>();
            EventQueueConfiguration config = dataCache.GetCache<EventQueueConfiguration>("EventQueueConfig", level: Core.Config.ProviderLevel.High);
            if (config == null)
            {
                string filePath = string.Concat(Globals.HostMapPath, configFilePath);
                if (!File.Exists(filePath))
                {
                    config = new EventQueueConfiguration()
                    {
                        PublishedEvents = new Dictionary<string, PublishedEvent>(),
                        EventQueueSubscribers = new Dictionary<string, SubscriberInfo>()
                    };
                    //SubscriberInfo subscriber = new SubscriberInfo("App Core");
                    //config.RegisterEventSubscription("Application_Start", subscriber);
                    //config.RegisterEventSubscription("Application_Start_FirstRequest", subscriber);
                    config.SaveConfig(filePath);
                }
                else
                {
                    config = new EventQueueConfiguration();
                    config.Deserialize(File.ReadAllText(filePath));
                    dataCache.SetCache("EventQueueConfig", config, level: Core.Config.ProviderLevel.High, dependency: new FileCacheDependency(filePath));
                }
            }
            return config;
        }

        public void Save()
        {
            var filePath = string.Concat(Globals.HostMapPath, configFilePath);
            this.SaveConfig(filePath);
        }

        internal void SaveConfig(string filePath)
        {
            var dir = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            StreamWriter oStream = File.CreateText(filePath);
            oStream.WriteLine(this.Serialize());
            oStream.Close();
            var dataCache = Extensions.GetService<IDataCacheManager>();
            dataCache.SetCache("EventQueueConfig", this, dependency: new FileCacheDependency(filePath));
        }

        private string Serialize()
        {
            XmlWriterSettings settings = new XmlWriterSettings()
            {
                ConformanceLevel = ConformanceLevel.Document,
                Indent = true,
                CloseOutput = true,
                OmitXmlDeclaration = false
            };
            StringBuilder sb = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(sb, settings);
            writer.WriteStartElement("EventQueueConfig");
            writer.WriteStartElement("PublishedEvents");
            foreach (string key in this.PublishedEvents.Keys)
            {
                writer.WriteStartElement("Event");
                writer.WriteElementString("EventName", this.PublishedEvents[key].EventName);
                writer.WriteElementString("EventThresholdSeconds", this.PublishedEvents[key].EventThresholdSeconds.ToString());
                writer.WriteElementString("Subscribers", this.PublishedEvents[key].Subscribers);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
            writer.WriteStartElement("EventQueueSubscribers");
            foreach (string str in this.EventQueueSubscribers.Keys)
            {
                writer.WriteStartElement("Subscriber");
                writer.WriteElementString("ID", this.EventQueueSubscribers[str].ID);
                writer.WriteElementString("Name", this.EventQueueSubscribers[str].Name);
                writer.WriteElementString("Address", this.EventQueueSubscribers[str].Address);
                writer.WriteElementString("Description", this.EventQueueSubscribers[str].Description);
                writer.WriteElementString("PrivateKey", this.EventQueueSubscribers[str].PrivateKey);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.Close();
            return sb.ToString();
        }
    }
}