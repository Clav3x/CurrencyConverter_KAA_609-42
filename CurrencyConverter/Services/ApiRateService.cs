using System.Globalization;
using System.Net;
using System.Text.Json;
using CurrencyConverter.Models;

namespace CurrencyConverter.Services;

public class ApiRateService : IRateService
{
    private HttpClient _httpClient;

    public ApiRateService()
    {
        _httpClient = new HttpClient();
    }

    public async Task<DailyRates?> GetRatesAsync(DateTime date)
    {
        string url = "http://localhost:5050/archive/"
                     + date.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture)
                     + "/daily_json.js";

        HttpResponseMessage response = await _httpClient.GetAsync(url);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        string json = await response.Content.ReadAsStringAsync();
        DailyRates? rates = JsonSerializer.Deserialize<DailyRates>(json);
        return rates;
    }
}