using StackExchange.Redis;
using System;

namespace FeedRssClient
{
    class Program
    {
        private const string RedisConnectionString = "localhost";
        private static ConnectionMultiplexer connection = ConnectionMultiplexer.Connect(RedisConnectionString);
        private const string publishChannel = "client-channel";
        private const string subscriberChannel = "rss-channel";
        private static string feedUrl = string.Empty;

        static void Main()
        {
            Console.WriteLine("FeedRssClient\r\n");
            Console.WriteLine($"Please enter the RSS feed that you want to follow: ");
            feedUrl = Console.ReadLine();

            var pubSubConnection = connection.GetSubscriber();

            pubSubConnection.Publish(publishChannel, $"{feedUrl}");
            var subscriberChannelMessageQueue = pubSubConnection.Subscribe(subscriberChannel);
            Console.WriteLine($"List of content: \r\n");
            subscriberChannelMessageQueue.OnMessage(message =>
            {
                Console.WriteLine(message);
            });
            Console.ReadLine();
        }
    }
}
