using CurrencyConverter.Models;
using CurrencyConverter.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CurrencyConverter.Tests;

[TestClass]
public class MainViewModelTests
{
    private DailyRates MakeRates(DateTime date, double usdValue)
    {
        DailyRates rates = new DailyRates();
        rates.Date = date;
        rates.Valute = new Dictionary<string, Valute>();

        Valute usd = new Valute();
        usd.CharCode = "USD";
        usd.Nominal = 1;
        usd.Name = "Доллар США";
        usd.Value = usdValue;
        rates.Valute.Add("USD", usd);

        return rates;
    }

    private MainViewModel MakeVmWithUsd80()
    {
        DateTime date = new DateTime(2026, 1, 15);
        DailyRates rates = MakeRates(date, 80);

        Dictionary<DateTime, DailyRates> byDate = new Dictionary<DateTime, DailyRates>();
        byDate.Add(date, rates);

        MainViewModel vm = new MainViewModel(new FakeRateService(byDate));
        vm.SelectedDate = date;
        return vm;
    }

    [TestMethod]
    public void Подсказка_ПоказываетДатуНайденногоКурса_ЕслиНаВыбраннуюДатуКурсовНет()
    {
        DateTime found = new DateTime(2026, 1, 15);
        DailyRates rates = MakeRates(found, 80);

        Dictionary<DateTime, DailyRates> byDate = new Dictionary<DateTime, DailyRates>();
        byDate.Add(found, rates);

        MainViewModel vm = new MainViewModel(new FakeRateService(byDate));
        vm.SelectedDate = new DateTime(2026, 1, 20);

        Assert.AreEqual("Курс на 15.01.2026", vm.Hint);
    }

    [TestMethod]
    public void Подсказка_СообщаетОбОтсутствииКурсов_ЕслиНичегоНеНашлось()
    {
        MainViewModel vm = new MainViewModel(new FakeRateService(new Dictionary<DateTime, DailyRates>()));

        vm.SelectedDate = new DateTime(2026, 1, 20);

        Assert.AreEqual("Курсы не нашлись за 14 дней до 20.01.2026", vm.Hint);
    }

    [TestMethod]
    public void Конвертация_ПереводитИзРублейВДоллары_ПослеВводаСуммы()
    {
        MainViewModel vm = MakeVmWithUsd80();

        vm.SelectedCurrency1 = "RUB";
        vm.SelectedCurrency2 = "USD";
        vm.Amount1 = "1000";

        Assert.AreEqual("12.5", vm.Amount2);
    }

    [TestMethod]
    public void Конвертация_ПереводитИзДолларовВРубли_ПослеВводаСуммы()
    {
        MainViewModel vm = MakeVmWithUsd80();

        vm.SelectedCurrency1 = "USD";
        vm.SelectedCurrency2 = "RUB";
        vm.Amount1 = "100";

        Assert.AreEqual("8000", vm.Amount2);
    }

    [TestMethod]
    public void СписокВалют_СодержитРубль_ПослеЗагрузки()
    {
        MainViewModel vm = MakeVmWithUsd80();

        Assert.AreEqual("RUB, USD", string.Join(", ", vm.Currencies));
    }
}