using System.Text.Json.Serialization;
namespace CurrencyConverter.Models;

public class Valute
{
    [JsonPropertyName("CharCode")]
    public string CharCode { get; set; }
    
    [JsonPropertyName("Nominal")]
    public int Nominal { get; set; }
    
    [JsonPropertyName("Name")]
    public string Name { get; set; }
    
    [JsonPropertyName("Value")]
    public double Value { get; set; }
}