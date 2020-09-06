using Microsoft.Toolkit.Parsers.Rss;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace FeedRssPublisher
{
    class Program
    {
        private const string RedisConnectionString = "localhost";
        private static ConnectionMultiplexer connection = ConnectionMultiplexer.Connect(RedisConnectionString);
        private const string publishChannel = "rss-channel";
        private const string subscriberChannel = "client-channel";

        static void Main()
        {
            Console.WriteLine("FeedRssPublisher\r\n");
            var feed = new Feed();
            var pubSubConnection = connection.GetSubscriber();
            var feedUrl = string.Empty;
            var subscriberChannelMessageQueue = pubSubConnection.Subscribe(subscriberChannel);

            subscriberChannelMessageQueue.OnMessage(async message =>
            {
                feedUrl = message.ToString().Remove(0, subscriberChannel.Length + 1);
                var rss = await feed.ParseRSSAsync(feedUrl);
                Console.WriteLine($"Feed Received: {feedUrl}\r\n");
                if (rss != null)
                {
                    Console.WriteLine("Start publishing contents ...");
                    foreach (var item in rss)
                    {
                        pubSubConnection.Publish(publishChannel, $"{item.Title}" + $"\r\n{item.Summary}" + $"\r\n{item.FeedUrl}\r\n");
                    }
                }
            });

            Console.ReadLine();

        }

        class Feed
        {
            public async Task<IEnumerable<RssSchema>> ParseRSSAsync(string feed)
            {
                IEnumerable<RssSchema> rss = null;

                using (var client = new HttpClient())
                {
                    try
                    {
                        feed = await client.GetStringAsync(feed);
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }

                if (feed != null)
                {
                    var parser = new RssParser();
                    rss = parser.Parse(feed);
                }

                return rss;
            }
        }
    }
}
