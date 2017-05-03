using System;
using ConcurrentCollections;

namespace WebSocketHub
{
    public class ConnectionList
    {
        private readonly ConcurrentHashSet<Connection> _connectedClients = new ConcurrentHashSet<Connection>();
        
        public void AddClient(Connection client)
        {
            _connectedClients.Add(client);
        }

        public void RemoveClient(Connection client)
        {
            _connectedClients.TryRemove(client);
        }
        
        public class ClientNotFoundException : Exception
        {
            public ClientNotFoundException()
            {
            }

            public ClientNotFoundException(string message) : base(message)
            {
            }

            public ClientNotFoundException(string message, Exception innerException) : base(message, innerException)
            {
            }
        }
    }
}