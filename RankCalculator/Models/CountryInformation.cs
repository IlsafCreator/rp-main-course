using RankCalculator.Enums;

namespace RankCalculator.Models;

public class CountryInformation
{
    public CountryInformation(string id, CountryEnum countryEnum)
    {
        Id = id;
        CountryEnum = countryEnum;
    }
    public string Id { get; init; }
    public CountryEnum CountryEnum { get; init; }
}