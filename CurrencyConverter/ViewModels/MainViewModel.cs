using System.Collections.ObjectModel;
using CurrencyConverter.Models;
using CurrencyConverter.Services;

namespace CurrencyConverter.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly IRateService _rateService;
    private bool _isConverting;


    public ObservableCollection<Valute> Currencies { get; } = new();
    

    private DateTime _selectedDate = DateTime.Today;
    public DateTime SelectedDate
    {
        get => _selectedDate;
        set
        {
            SetProperty(ref _selectedDate, value);
            _ = LoadRatesAsync();
        }
    }

    private string _amount1 = "";
    public string Amount1
    {
        get => _amount1;
        set
        {
            SetProperty(ref _amount1, value);
            Convert(1);
        }
    }

    private string _amount2 = "";
    public string Amount2
    {
        get => _amount2;
        set
        {
            SetProperty(ref _amount2, value);
            Convert(2);
        }
    }

    private Valute? _selectedCurrency1;
    public Valute? SelectedCurrency1
    {
        get => _selectedCurrency1;
        set
        {
            SetProperty(ref _selectedCurrency1, value);
            Convert(1);
        }
    }

    private Valute? _selectedCurrency2;
    public Valute? SelectedCurrency2
    {
        get => _selectedCurrency2;
        set
        {
            SetProperty(ref _selectedCurrency2, value);
            Convert(1);
        }
    }

    private string _hint = "Загрузка курсов...";
    public string Hint
    {
        get => _hint;
        set => SetProperty(ref _hint, value);
    }
    

    public MainViewModel(IRateService rateService)
    {
        _rateService = rateService;
        _ = LoadRatesAsync();
    }
    

    public async Task LoadRatesAsync()
    {
        DateTime day = SelectedDate;

        for (int attempt = 0; attempt < 14; attempt++)
        {
            DailyRates? rates = await _rateService.GetRatesAsync(day);

            if (rates is null)
            {
                Hint = $"Нет данных на {day:dd.MM.yyyy}, пробую день раньше...";
                day = day.AddDays(-1);
                continue;
            }

            Currencies.Clear();
            foreach (Valute v in rates.Valute.Values)
                Currencies.Add(v);
            AddRub();

     
            if (SelectedCurrency1 is null || !Currencies.Contains(SelectedCurrency1))
                SelectedCurrency1 = Currencies.FirstOrDefault(c => c.CharCode == "RUB");
            if (SelectedCurrency2 is null || !Currencies.Contains(SelectedCurrency2))
                SelectedCurrency2 = Currencies.FirstOrDefault(c => c.CharCode == "USD");

            Hint = $"Курсы на {day:dd.MM.yyyy}";
            Convert(1);
            return;
        }

        Hint = "Не удалось загрузить курсы даже за 14 дней назад";
    }

    private void AddRub()
    {
        Currencies.Insert(0, new Valute
        {
            CharCode = "RUB",
            Nominal = 1,
            Name = "Российский рубль",
            Value = 1.0
        });
    }

  

    public void Convert(int direction)
    {
        if (_isConverting) return;

        Valute? c1 = SelectedCurrency1;
        Valute? c2 = SelectedCurrency2;
        if (c1 is null || c2 is null) return;

        _isConverting = true;
        try
        {
            if (direction == 1)
            {
                if (double.TryParse(Amount1, out double a1))
                {
                    double rubles = a1 * c1.Value / c1.Nominal;
                    double result = Math.Round(rubles * c2.Nominal / c2.Value, 4);
                    Amount2 = result.ToString();
                }
            }
            else
            {
                if (double.TryParse(Amount2, out double a2))
                {
                    double rubles = a2 * c2.Value / c2.Nominal;
                    double result = Math.Round(rubles * c1.Nominal / c1.Value, 4);
                    Amount1 = result.ToString();
                }
            }
        }
        finally
        {
            _isConverting = false;
        }
    }
}