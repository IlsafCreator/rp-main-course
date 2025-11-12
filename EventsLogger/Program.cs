using System.Text;
using NATS.Client;
using StackExchange.Redis;

namespace EventsLogger;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("EventsLogger started");
        
        ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost:6379,abortConnect=false");
        IDatabase db = redis.GetDatabase();
        
        ConnectionFactory cf = new ConnectionFactory();
        using IConnection c = cf.CreateConnection();

        var rankSubscribe = c.SubscribeAsync("rankCalculated", "events_logger", (sender, args) =>
        {
            string m = Encoding.UTF8.GetString(args.Message.Data);
            string rankValue = db.StringGet("RANK-" + m);
            Console.WriteLine($"Time: {DateTime.UtcNow}\n" +
                              $"event: Rank calculated\n" +
                              $"id: {m}\n" +
                              $"rank value: {rankValue}" +
                              $"\n--------------------------------------------");
        });
        
        var similaritySubscribe = c.SubscribeAsync("similarityCalculated", "events_logger", (sender, args) =>
        {
            string m = Encoding.UTF8.GetString(args.Message.Data);
            string similarityValue = db.StringGet("SIMILARITY-" + m);
            Console.WriteLine($"Time: {DateTime.UtcNow}\n" +
                              $"event: Similarity calculated\n" +
                              $"id: {m}\n" +
                              $"similarity value: {similarityValue}" +
                              $"\n--------------------------------------------");
        });

        rankSubscribe.Start();
        similaritySubscribe.Start();

        Console.WriteLine("EventsLogger listening...\n");
        Console.ReadLine();

        rankSubscribe.Unsubscribe();
        similaritySubscribe.Unsubscribe();

        c.Drain();
        c.Close();
    }
}