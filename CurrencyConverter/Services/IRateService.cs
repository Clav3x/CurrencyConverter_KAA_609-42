using CurrencyConverter.Models;

namespace CurrencyConverter.Services;

public interface IRateService
{
    Task<DailyRates?> GetRatesAsync(DateTime date);
}