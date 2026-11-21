using System.Net;
using System.Text.Json;
using CurrencyConverter.Models;

namespace CurrencyConverter.Services;

public class ApiRateService : IRateService
{
    private readonly HttpClient _client;

    public ApiRateService(HttpClient httpClient)
    {
        _client = httpClient;
    }

    public async Task<DailyRates?> GetRatesAsync(DateTime date)
    {
        try
        {
            string url = $"http://localhost:5050/archive/{date:yyyy}/{date:MM}/{date:dd}/daily_json.js";

            using HttpResponseMessage response = await _client.GetAsync(url);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<DailyRates>(json);
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }
}