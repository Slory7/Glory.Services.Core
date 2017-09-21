[**Glory.Services.Core**](https://www.nuget.org/packages/Glory.Services.Core) is a .Net Core provider pattern wrapper for essential services: **Data Cache**, **Event Queue** and **Data Store**.
Two providers included in these services: a simple single app used provider, and a distributed app used provider.
# Data Cache
* Expired callback supported
* Most frequently used APIs
* Distributed provider: [**Redis**](https://www.nuget.org/packages/Glory.Provider.DataCache.Redis.Core)
# Event Queue
* Strong typed subscribers
* Dynamic typed subscribers
* External app subscribers via http.
* Distributed provider: [**RabbitMQ**](https://www.nuget.org/packages/Glory.Provider.EventQueue.RabbitMQ.Core)
# Data Store
* A Stage data store: for hot feed messages.
* Distributed provider: [**MongoDB**](https://www.nuget.org/packages/Glory.Provider.DataStore.MongoDB.Core)
#### Thanks to DNN and eShopOnContainers.
