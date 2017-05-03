using System;

namespace WebSocketHub.HubMessages
{
    public class SubscriptionData
    {
        public string Topic { get; }
        public SubscriptionData(string topic)
        {
            if (string.IsNullOrEmpty(topic))
            {
                throw new ArgumentNullException(nameof(topic));
            }
            Topic = topic;
        }
    }
}