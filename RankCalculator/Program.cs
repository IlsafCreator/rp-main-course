using System.Text;
using System.Text.Json;
using NATS.Client;
using RankCalculator.Enums;
using RankCalculator.Models;
using StackExchange.Redis;

namespace RankCalculator;

class Program
{
    static void Main(string[] args)
    {
	    
        Console.WriteLine("Rank calculator started");
        
        ConnectionFactory cf = new ConnectionFactory();
        using IConnection c = cf.CreateConnection();
		
		var s = c.SubscribeAsync("valuator.processing.rank", "rank_calculator", (sender, args) =>
        {
			Console.WriteLine("Rank calculator process start...");

            string m = Encoding.UTF8.GetString(args.Message.Data);
            CountryInformation info = JsonSerializer.Deserialize<CountryInformation>(m);
            
            string countryName= info.CountryEnum switch
            {
	            CountryEnum.RUS => "RUS",
	            CountryEnum.EU => "EU",
	            _ => "OTHER"
            };

            string dbEnvironmentVariable = $"DB_{countryName}";
            string? dbConnection = Environment.GetEnvironmentVariable(dbEnvironmentVariable);
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(dbConnection);
            IDatabase db = redis.GetDatabase();
            
            string textKey = "TEXT-" + info.Id;
            string text = db.StringGet(textKey)!;
            
            string rankKey = "RANK-" + info.Id;
            double rankValue = (double)text.Count(_ => !char.IsLetter(_))/text.Length;
            
            db.StringSet(rankKey, rankValue.ToString());

            Console.WriteLine($"LOOKUP: {info.Id}, {countryName}.");

			Console.WriteLine("Rank calculator process completed!");
			
			byte[] rankData = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new DataInformation(info.Id, rankValue)));
			c.Publish("rankCalculated", rankData);
        });
        
        s.Start();
        
        Console.WriteLine("Press Enter to exit");
        Console.ReadLine();

        s.Unsubscribe();

        c.Drain();
        c.Close();
    }
}
