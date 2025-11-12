using System.Text;
using System.Text.Json;
using EventsLogger.Models;
using NATS.Client;

namespace EventsLogger;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("EventsLogger started");
        
        ConnectionFactory cf = new ConnectionFactory();
        using IConnection c = cf.CreateConnection();

        var rankSubscribe = c.SubscribeAsync("rankCalculated", "events_logger", (sender, args) =>
        {
            string m = Encoding.UTF8.GetString(args.Message.Data);
            DataInformation dataInfo = JsonSerializer.Deserialize<DataInformation>(m);
            Console.WriteLine($"Time: {DateTime.UtcNow}\n" +
                              $"event: Rank calculated\n" +
                              $"id: {dataInfo.Id}\n" +
                              $"rank value: {dataInfo.Data}" +
                              $"\n--------------------------------------------");
        });
        
        var similaritySubscribe = c.SubscribeAsync("similarityCalculated", "events_logger", (sender, args) =>
        {
            string m = Encoding.UTF8.GetString(args.Message.Data);
            DataInformation dataInfo = JsonSerializer.Deserialize<DataInformation>(m);
            Console.WriteLine($"Time: {DateTime.UtcNow}\n" +
                              $"event: Similarity calculated\n" +
                              $"id: {dataInfo.Id}\n" +
                              $"similarity value: {dataInfo.Data}" +
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