using System.Globalization;
using CurrencyConverter.Models;     
using CurrencyConverter.Services; 
using CurrencyConverter.ViewModels; 
using Moq;
using Xunit;
using Assert = Xunit.Assert;

namespace CurrencyConverter.Tests;

public class MainViewModelTests
{
    private static DailyRates MakeRates(double usd = 80.0) => new()
    {
        Valute = new Dictionary<string, Valute>
        {
            ["USD"] = new Valute { CharCode = "USD", Nominal = 1, Name = "Доллар США", Value = usd }
        }
    };


    private static MainViewModel CreateVm(Mock<IRateService> mock)
    {
        var vm = new MainViewModel(mock.Object);
        
        mock.Invocations.Clear();

        return vm;
    }

    [Fact]
    public async Task LoadRates_FillsCurrencies_AndPutsRubFirst()
    {
        var mock = new Mock<IRateService>();
        var vm = CreateVm(mock);

        mock.Setup(s => s.GetRatesAsync(It.IsAny<DateTime>())).ReturnsAsync(MakeRates());

        await vm.LoadRatesAsync();

        Assert.Equal(2, vm.Currencies.Count);           
        Assert.Equal("RUB", vm.Currencies[0].CharCode);
    }

    [Fact]
    public async Task Convert_Direction1_RubToUsd_UsesRate()
    {
        var mock = new Mock<IRateService>();
        var vm = CreateVm(mock);
        mock.Setup(s => s.GetRatesAsync(It.IsAny<DateTime>())).ReturnsAsync(MakeRates(usd: 80));

        await vm.LoadRatesAsync();
        vm.SelectedCurrency1 = vm.Currencies.First(c => c.CharCode == "RUB");
        vm.SelectedCurrency2 = vm.Currencies.First(c => c.CharCode == "USD");
        vm.Amount1 = "100";

        vm.Convert(1);

        double actual = double.Parse(vm.Amount2, CultureInfo.CurrentCulture);
        Assert.Equal(1.25, actual, 4);                
    }

    [Fact]
    public async Task Convert_Direction2_UsdToRub_IsSymmetric()
    {
        var mock = new Mock<IRateService>();
        var vm = CreateVm(mock);
        mock.Setup(s => s.GetRatesAsync(It.IsAny<DateTime>())).ReturnsAsync(MakeRates(usd: 80));

        await vm.LoadRatesAsync();
        vm.SelectedCurrency1 = vm.Currencies.First(c => c.CharCode == "RUB");
        vm.SelectedCurrency2 = vm.Currencies.First(c => c.CharCode == "USD");
        vm.Amount2 = "100";

        vm.Convert(2);

        double actual = double.Parse(vm.Amount1, CultureInfo.CurrentCulture);
        Assert.Equal(8000, actual, 4);                
    }

    [Fact]
    public async Task LoadRates_RollsBack_WhenDateUnavailable()
    {
        var mock = new Mock<IRateService>();
        var vm = CreateVm(mock);                       

        mock.Setup(s => s.GetRatesAsync(DateTime.Today)).ReturnsAsync((DailyRates?)null);
        mock.Setup(s => s.GetRatesAsync(DateTime.Today.AddDays(-1))).ReturnsAsync(MakeRates());

        await vm.LoadRatesAsync();
        
        mock.Verify(s => s.GetRatesAsync(It.IsAny<DateTime>()), Times.Exactly(2));
        mock.VerifyNoOtherCalls();
        Assert.Equal(2, vm.Currencies.Count);
    }

    [Fact]
    public async Task Convert_Recalculates_WhenRatesChange()
    {
        var mock = new Mock<IRateService>();
        var vm = CreateVm(mock);

        mock.Setup(s => s.GetRatesAsync(It.IsAny<DateTime>())).ReturnsAsync(MakeRates(usd: 80));
        await vm.LoadRatesAsync();
        vm.SelectedCurrency1 = vm.Currencies.First(c => c.CharCode == "RUB");
        vm.SelectedCurrency2 = vm.Currencies.First(c => c.CharCode == "USD");
        vm.Amount1 = "100";
        vm.Convert(1);
        double first = double.Parse(vm.Amount2, CultureInfo.CurrentCulture);
        Assert.Equal(1.25, first, 4);

        mock.Setup(s => s.GetRatesAsync(It.IsAny<DateTime>())).ReturnsAsync(MakeRates(usd: 40));
        await vm.LoadRatesAsync();
        vm.Convert(1);
        double second = double.Parse(vm.Amount2, CultureInfo.CurrentCulture);
        Assert.Equal(2.5, second, 4);               
        Assert.NotEqual(first, second);
    }

    [Fact]
    public async Task Convert_GarbageInput_DoesNotBreak()
    {
        var mock = new Mock<IRateService>();
        var vm = CreateVm(mock);
        mock.Setup(s => s.GetRatesAsync(It.IsAny<DateTime>())).ReturnsAsync(MakeRates());

        await vm.LoadRatesAsync();
        vm.Amount1 = "12abc";

        vm.Convert(1);

        Assert.True(string.IsNullOrEmpty(vm.Amount2));
    }
}