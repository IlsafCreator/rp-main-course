using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NATS.Client;
using StackExchange.Redis;
using Valuator.Enums;
using Valuator.Models;

namespace Valuator.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _db;

    public IndexModel(ILogger<IndexModel> logger, IConnectionMultiplexer redis)
    {
        _logger = logger;
        _redis = redis;
        _db = _redis.GetDatabase();
    }

    public void OnGet()
    {

    }

    public IActionResult OnPost(string text, CountryEnum country)
    {
        if (text == null)
            return Redirect($"index");

        _logger.LogDebug(text);

        string id = Guid.NewGuid().ToString();
        string countryName= country switch
        {
            CountryEnum.RUS => "RUS",
            CountryEnum.EU => "EU",
            _ => "OTHER"
        };
        _db.StringSet(id, countryName);

        string dbEnvironmentVariable = $"DB_{countryName}";
        string? dbConnection = Environment.GetEnvironmentVariable(dbEnvironmentVariable);
        ConnectionMultiplexer countryRedis = ConnectionMultiplexer.Connect(dbConnection);
        IDatabase db = countryRedis.GetDatabase();
        
        string similarityKey = "SIMILARITY-" + id;
        int similarityValue = CalculateSimilarity(text, db, countryRedis);
        db.StringSet(similarityKey, similarityValue);
        
        string textKey = "TEXT-" + id;
        db.StringSet(textKey, text);
        
        Console.WriteLine($"LOOKUP: {id}, {countryName}.");
        
        ConnectionFactory cf = new ConnectionFactory();
        
        using (IConnection c = cf.CreateConnection())
        {
            byte[] countryData = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new CountryInformation(id, country)));
            byte[] similarityData = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new DataInformation(id, similarityValue)));
            c.Publish("valuator.processing.rank", countryData);
            c.Publish("similarityCalculated", similarityData);
            
            c.Drain();

            c.Close();
        }
        
        Thread.Sleep(1000);

        return Redirect($"summary?id={id}&countryEnum={country}");
    }

    private int CalculateSimilarity(string text, IDatabase db, IConnectionMultiplexer redis)
    {
        var keys = redis.GetServer(redis.GetEndPoints().First()).Keys();
        List<string> values = keys.Select(_ => db.StringGet(_).ToString()).ToList();
        return values.Any(_ => _ == text) ? 1 : 0;
    }
}
