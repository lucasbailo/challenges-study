using EquipmentAggregator.Api.Suppliers;

namespace EquipmentAggregator.Tests;

public class HeavyMachClientTests
{
    [Theory]
    [InlineData(420000L, "week", 600.00)]  // exemplo do enunciado
    [InlineData(85000L, "day", 850.00)]
    [InlineData(100000L, "week", 142.86)]  // 1000 / 7 = 142.857... → arredonda
    [InlineData(70000L, "WEEK", 100.00)]   // unit em maiúsculas
    public void ToDailyRate_ConvertsCentsAndUnitToDailyUsd(long cents, string unit, double expected)
    {
        var rate = HeavyMachClient.ToDailyRate(new HeavyMachPricing(cents, "USD", unit));

        Assert.Equal((decimal)expected, rate);
    }

    [Fact]
    public void ToDailyRate_UnknownUnit_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => HeavyMachClient.ToDailyRate(new HeavyMachPricing(1000, "USD", "month")));
    }

    [Theory]
    [InlineData("AVAILABLE", true)]
    [InlineData("RENTED", false)]
    [InlineData("MAINTENANCE", false)]
    public void ToEquipment_MapsStatusToIsAvailable(string status, bool expected)
    {
        var item = new HeavyMachItem(4471, "Bulldozer D6", new HeavyMachPricing(420000, "USD", "week"), status);

        var equipment = HeavyMachClient.ToEquipment(item);

        Assert.Equal(expected, equipment.IsAvailable);
    }

    [Fact]
    public void ToEquipment_MapsAllFields()
    {
        var item = new HeavyMachItem(4471, "Bulldozer D6", new HeavyMachPricing(420000, "USD", "week"), "AVAILABLE");

        var equipment = HeavyMachClient.ToEquipment(item);

        Assert.Equal("4471", equipment.Id);
        Assert.Equal("Bulldozer D6", equipment.Name);
        Assert.Equal(600m, equipment.DailyRate);
        Assert.Equal("HeavyMach", equipment.Supplier);
    }
}
