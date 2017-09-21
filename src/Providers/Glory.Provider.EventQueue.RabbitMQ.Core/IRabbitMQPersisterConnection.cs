using RabbitMQ.Client;
using System;

namespace Glory.Provider.EventQueue.RabbitMQ.Core
{
    public interface IRabbitMQPersistentConnection
        : IDisposable
    {
        bool IsConnected { get; }

        bool TryConnect();

        IModel CreateModel();
    }
}
