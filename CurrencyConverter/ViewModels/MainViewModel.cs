using System.Globalization;
using System.Linq;
using CurrencyConverter.Models;
using CurrencyConverter.Services;

namespace CurrencyConverter.ViewModels;

public class MainViewModel : ViewModelBase
{
    private IRateService _rateService;

    private DailyRates? _rates;
    private DateTime _selectedDate;
    private string? _hint;
    private List<string>? _currencies;
    
    private string? _amount1;
    private string? _amount2;
    private string? _selectedCurrency1;
    private string? _selectedCurrency2;
    private bool _isConverting;

    public DailyRates? Rates
    {
        get { return _rates; }
        set { SetProperty(ref _rates, value); }
    }
    
    public List<string>? Currencies
    {
        get { return _currencies; }
        set { SetProperty(ref _currencies, value); }
    }
    
    public string? Hint
    {
        get { return _hint; }
        set { SetProperty(ref _hint, value); }
    }
    

    public DateTime SelectedDate
    {
        get { return _selectedDate; }
        set
        {
            if (SetProperty(ref _selectedDate, value))
            {
                _ = LoadRatesAsync();
            }
        }
    }

    
    public MainViewModel()
        : this(new ApiRateService())
    {
    }

    public MainViewModel(IRateService rateService)
    {
        _rateService = rateService;
        SelectedDate = DateTime.Today;
    }

    public async Task LoadRatesAsync()
    {
        DailyRates? rates = null;
        DateTime date = SelectedDate;
        int daysChecked = 0;

        while (rates == null && daysChecked < 14)
        {
            rates = await _rateService.GetRatesAsync(date);

            if (rates == null)
            {
                date = date.AddDays(-1);
                daysChecked++;
            }
        }

        if (rates == null)
        {
            Hint = "Курсы не нашлись за 14 дней до " + SelectedDate.ToString("dd.MM.yyyy");
            
            return;
        }

        AddRub(rates);

        Rates = rates;
        Currencies = rates.Valute.Keys.OrderBy(x => x).ToList();
        Hint = "Курс на " + rates.Date.ToString("dd.MM.yyyy");
        if (SelectedCurrency1 == null)
        {
            SelectedCurrency1 = "RUB";
        }

        if (SelectedCurrency2 == null)
        {
            SelectedCurrency2 = "USD";
        }

        Convert(1);
    }

    private void AddRub(DailyRates rates)
    {
        if (rates.Valute.ContainsKey("RUB"))
        {
            return;
        }

        Valute rub = new Valute();
        rub.CharCode = "RUB";
        rub.Nominal = 1;
        rub.Name = "Российский рубль";
        rub.Value = 1;
        rates.Valute.Add("RUB", rub);
        
    }
    
    
    public string? Amount1
    {
        get { return _amount1; }
        set
        {
            if (SetProperty(ref _amount1, value))
            {
                Convert(1);
            }
        }
    }
    public string? Amount2
    {
        get { return _amount2; }
        set
        {
            if (SetProperty(ref _amount2, value))
            {
                Convert(2);
            }
        }
    }
    
    public string? SelectedCurrency1
    {
        get { return _selectedCurrency1; }
        set
        {
            if (SetProperty(ref _selectedCurrency1, value))
            {
                Convert(1);
            }
        }
    }
    public string? SelectedCurrency2
    {
        get { return _selectedCurrency2; }
        set
        {
            if (SetProperty(ref _selectedCurrency2, value))
            {
                Convert(2);
            }
        }
    }
    
    private void Convert(int direction)
    {
        if (_isConverting || Rates == null)
        {
            return;
        }

        if (direction == 1)
        {
            if (Amount1 == null || SelectedCurrency1 == null || SelectedCurrency2 == null)
            {
                return;
            }

            _isConverting = true;

            string text = Amount1.Replace(",", ".");
            double amount;

            if (double.TryParse(text, CultureInfo.InvariantCulture, out amount))
            {
                Valute v1 = Rates.Valute[SelectedCurrency1];
                Valute v2 = Rates.Valute[SelectedCurrency2];

                double price1 = v1.Value / v1.Nominal;
                double price2 = v2.Value / v2.Nominal;

                double result = amount * price1 / price2;

                Amount2 = Math.Round(result, 2).ToString(CultureInfo.InvariantCulture);
            }
            else
            {
                Amount2 = "";
            }

            _isConverting = false;
        }

        if (direction == 2)
        {
            if (Amount2 == null || SelectedCurrency2 == null || SelectedCurrency1 == null)
            {
                return;
            }

            _isConverting = true;

            string text = Amount2.Replace(",", ".");
            double amount;

            if (double.TryParse(text, CultureInfo.InvariantCulture, out amount))
            {
                Valute v1 = Rates.Valute[SelectedCurrency1];
                Valute v2 = Rates.Valute[SelectedCurrency2];

                double price1 = v1.Value / v1.Nominal;
                double price2 = v2.Value / v2.Nominal;

                double result = amount * price2 / price1;

                Amount1 = Math.Round(result, 2).ToString(CultureInfo.InvariantCulture);
            }
            else
            {
                Amount1 = "";
            }

            _isConverting = false;
        }
    }
}