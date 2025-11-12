namespace Valuator.Models;

public class DataInformation
{
    public DataInformation(string id, double data)
    {
        Id = id;
        Data = data;
    }
    public string Id { get; init; }
    public double Data { get; init; }
}