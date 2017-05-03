using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using StackExchange.Redis;

namespace Timer.Service
{
    public class Clock
    {
        private event OnTickDelegate OnTick;
        private readonly System.Threading.Timer _timer;

        public Clock(OnTickDelegate onTickCallback)
        {
            _timer = new System.Threading.Timer(OnTickHappened, null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
            if (onTickCallback != null)
            {
                OnTick += onTickCallback;
            }
        }

        private void OnTickHappened(object state)
        {
            var localOnTick = OnTick;
            localOnTick?.Invoke(DateTime.Now);
        }

        public void Start()
        {
            _timer.Change(TimeSpan.Zero, TimeSpan.FromSeconds(1));
        }

        public void Stop()
        {
            _timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        }
    }

    public delegate void OnTickDelegate(DateTime currentTime);

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Setting up clock..");
            Console.WriteLine("Press enter to exit.");

            Console.WriteLine("Setting up redis connection...");
            var redis = ConnectionMultiplexer.Connect("localhost");
            var subscription = redis.GetSubscriber();

            var clock = new Clock(currentTime =>
            {
                var serializedMessage = JsonConvert.SerializeObject(new TimerMessage(currentTime), new JsonSerializerSettings()
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                });
                Console.WriteLine(serializedMessage);
                subscription.Publish("timer", serializedMessage);
            });

            clock.Start();
            Console.ReadLine();

            clock.Stop();
            redis.Dispose();
        }
    }

    class TimerMessage
    {
        public TimerMessage(DateTime currentTime)
        {
            MessageType = MessageType.Message;
            Data = new MessageData("timer", currentTime.ToString("O"));
        }
        public MessageType MessageType { get; }
        public MessageData Data { get; }
    }

    internal class MessageData
    {
        public MessageData(string topic, string message)
        {
            Topic = topic;
            Message = message;
        }

        public string Topic { get; }
        public string Message { get; }
    }

    internal enum MessageType
    {
        Subscribe,
        Unsubscribe,
        Message
    }
}