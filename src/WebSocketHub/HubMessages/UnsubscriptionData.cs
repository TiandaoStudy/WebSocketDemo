using System;

namespace WebSocketHub.HubMessages
{
    public class UnsubscriptionData
    {
        public string Topic { get; }
        public UnsubscriptionData(string topic)
        {
            if (string.IsNullOrEmpty(topic))
            {
                throw new ArgumentNullException(nameof(topic));
            }
            Topic = topic;
        }
    }
}
