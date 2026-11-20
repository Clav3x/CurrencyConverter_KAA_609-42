using System.Text.Json.Serialization;
namespace CurrencyConverter.Models;

public class DailyRates
{
    [JsonPropertyName("Date")]
    public DateTime Date { get; set; }
    [JsonPropertyName("Valute")]
    public Dictionary<string, Valute> Valute { get; set; }
}