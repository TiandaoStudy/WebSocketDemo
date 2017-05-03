namespace WebSocketHub.HubMessages
{
    public class HubMessage
    {
        public MessageType MessageType { get; set; }
        public dynamic Data { get; set; }
    }
}
