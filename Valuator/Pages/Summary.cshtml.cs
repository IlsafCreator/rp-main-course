using Microsoft.AspNetCore.Mvc.RazorPages;
using StackExchange.Redis;
using Valuator.Enums;

namespace Valuator.Pages;
public class SummaryModel : PageModel
{
    private readonly ILogger<SummaryModel> _logger;

    public SummaryModel(ILogger<SummaryModel> logger, IConnectionMultiplexer redis)
    {
        _logger = logger;
    }

    public double Rank { get; set; }
    public double Similarity { get; set; }

    public void OnGet(string id, CountryEnum countryEnum)
    {
        _logger.LogDebug(id);
        
        string countryName= countryEnum switch
        {
            CountryEnum.RUS => "RUS",
            CountryEnum.EU => "EU",
            _ => "OTHER"
        };

        string dbEnvironmentVariable = $"DB_{countryName}";
        string? dbConnection = Environment.GetEnvironmentVariable(dbEnvironmentVariable);
        ConnectionMultiplexer countryRedis = ConnectionMultiplexer.Connect(dbConnection);
        IDatabase db = countryRedis.GetDatabase();
        
        Console.WriteLine($"LOOKUP: {id}, {countryName}.");

        Rank = double.Parse(db.StringGet($"RANK-{id}"));
        Similarity = double.Parse(db.StringGet($"SIMILARITY-{id}"));
    }
}
