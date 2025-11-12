using System.Text;
using NATS.Client;
using StackExchange.Redis;

namespace RankCalculator;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Rank calculator started");

        ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost:6379,abortConnect=false");
        IDatabase db = redis.GetDatabase();
        
        ConnectionFactory cf = new ConnectionFactory();
        using IConnection c = cf.CreateConnection();
		
		var s = c.SubscribeAsync("valuator.processing.rank", "rank_calculator", (sender, args) =>
        {
			Console.WriteLine("Rank calculator process start...");

            string m = Encoding.UTF8.GetString(args.Message.Data);

            string textKey = "TEXT-" + m;
            string text = db.StringGet(textKey)!;
            
            string rankKey = "RANK-" + m;
            double rankValue = (double)text.Count(_ => !char.IsLetter(_))/text.Length;
            
            db.StringSet(rankKey, rankValue.ToString());

			Console.WriteLine("Rank calculator process completed!");
			
			byte[] data = Encoding.UTF8.GetBytes(m);
			c.Publish("rankCalculated", data);
        });
        
        s.Start();
        
        Console.WriteLine("Press Enter to exit");
        Console.ReadLine();

        s.Unsubscribe();

        c.Drain();
        c.Close();
    }
}
