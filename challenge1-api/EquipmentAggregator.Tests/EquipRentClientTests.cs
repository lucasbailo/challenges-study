using EquipmentAggregator.Api.Suppliers;

namespace EquipmentAggregator.Tests;

public class EquipRentClientTests
{
    [Fact]
    public void ToEquipment_MapsAllFields()
    {
        var item = new EquipRentItem("EX-001", "Excavator CAT 320", 850.0m, true, DateTime.UtcNow);

        var equipment = EquipRentClient.ToEquipment(item);

        Assert.Equal("EX-001", equipment.Id);
        Assert.Equal("Excavator CAT 320", equipment.Name);
        Assert.Equal(850.0m, equipment.DailyRate);
        Assert.True(equipment.IsAvailable);
        Assert.Equal("EquipRent", equipment.Supplier);
    }
}
