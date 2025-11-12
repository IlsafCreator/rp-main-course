using Valuator.Enums;

namespace Valuator.Models;

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