using EquipmentAggregator.Api.Models;

namespace EquipmentAggregator.Api.Suppliers;

// Espelha o JSON "cru" do fornecedor
public record EquipRentItem(string EquipmentId, string Desc, decimal DailyRateUsd, bool Available, DateTime LastUpdate);

public class EquipRentClient : ISupplierClient
{
    public const string SupplierName = "EquipRent";

    public string Name => SupplierName;

    private static readonly List<EquipRentItem> FakeData =
    [
        new("ER001", "Excavator", 150.00m, true, DateTime.UtcNow),
        new("ER002", "Bulldozer", 200.00m, true, DateTime.UtcNow),
        new("ER003", "Compact Skid Steer", 95.00m, true, DateTime.UtcNow),
        new("ER004", "Scissor Lift (32ft)", 85.00m, false, DateTime.UtcNow),
        new("ER005", "Boom Lift (60ft)", 1175.00m, true, DateTime.UtcNow),
        new("ER006", "Concrete Mixer", 45.00m, true, DateTime.UtcNow),
        new("ER007", "Industrial Air Compressor", 65.00m, true, DateTime.UtcNow),
        new("ER008", "Portable Generator 10kW", 55.00m, false, DateTime.UtcNow),
        new("ER009", "Plate Compactor", 40.00m, true, DateTime.UtcNow),
        new("ER010", "Hydraulic Breaker", 110.00m, true, DateTime.UtcNow),
    ];

    public Task<IReadOnlyList<Equipment>> GetEquipmentAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        var equipment = FakeData.Select(ToEquipment).ToList();

        return Task.FromResult<IReadOnlyList<Equipment>>(equipment);
    }

    // Já vem em USD por dia: o mapeamento é direto
    public static Equipment ToEquipment(EquipRentItem item) =>
        new(item.EquipmentId, item.Desc, item.DailyRateUsd, item.Available, SupplierName);
}
