using EquipmentAggregator.Api.Models;

namespace EquipmentAggregator.Api.Suppliers;

// Espelham o JSON "cru" do fornecedor
public record HeavyMachPricing(long AmountInCents, string Currency, string Unit);
public record HeavyMachItem(int Code, string Name, HeavyMachPricing Pricing, string Status);
public record HeavyMachResponse(IReadOnlyList<HeavyMachItem> Items);

public class HeavyMachClient : ISupplierClient
{
    public const string SupplierName = "HeavyMach";

    public string Name => SupplierName;

    private static readonly HeavyMachResponse FakeResponse = new(
    [
        new(4471, "Bulldozer D6", new(420000, "USD", "week"), "AVAILABLE"),
        new(4472, "Wheel Loader 950L", new(95000, "USD", "day"), "RENTED"),
        new(4473, "Motor Grader 140", new(1050000, "USD", "week"), "AVAILABLE"),
        new(4474, "Tower Crane", new(180000, "USD", "day"), "MAINTENANCE"),
        new(4475, "Mini Excavator 305", new(210000, "USD", "week"), "AVAILABLE"),
        new(4476, "Backhoe Loader 416", new(70000, "USD", "day"), "AVAILABLE"),
    ]);

    public Task<IReadOnlyList<Equipment>> GetEquipmentAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        var equipment = FakeResponse.Items.Select(ToEquipment).ToList();

        return Task.FromResult<IReadOnlyList<Equipment>>(equipment);
    }

    public static Equipment ToEquipment(HeavyMachItem item) =>
        new(
            item.Code.ToString(),
            item.Name,
            ToDailyRate(item.Pricing),
            string.Equals(item.Status, "AVAILABLE", StringComparison.OrdinalIgnoreCase),
            SupplierName);

    // centavos → dólar, semana → dia
    public static decimal ToDailyRate(HeavyMachPricing pricing)
    {
        var amount = pricing.AmountInCents / 100m;

        var daily = pricing.Unit.ToLowerInvariant() switch
        {
            "day" => amount,
            "week" => amount / 7,
            _ => throw new ArgumentOutOfRangeException(nameof(pricing), $"Unknown pricing unit '{pricing.Unit}'.")
        };

        return Math.Round(daily, 2, MidpointRounding.AwayFromZero);
    }
}
