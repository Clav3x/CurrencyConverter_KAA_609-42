using CurrencyConverter.Models;
using CurrencyConverter.Services;

namespace CurrencyConverter.Tests;

public class FakeRateService : IRateService
{
    private Dictionary<DateTime, DailyRates> _byDate;

    public FakeRateService(Dictionary<DateTime, DailyRates> byDate)
    {
        _byDate = byDate;
    }

    public Task<DailyRates?> GetRatesAsync(DateTime date)
    {
        if (_byDate.ContainsKey(date))
        {
            return Task.FromResult<DailyRates?>(_byDate[date]);
        }

        return Task.FromResult<DailyRates?>(null);
    }
}