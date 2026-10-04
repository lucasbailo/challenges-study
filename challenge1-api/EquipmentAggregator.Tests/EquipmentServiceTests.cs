using EquipmentAggregator.Api.Models;
using EquipmentAggregator.Api.Services;
using EquipmentAggregator.Api.Suppliers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace EquipmentAggregator.Tests;

public class EquipmentServiceTests
{
    private static EquipmentService CreateService(params ISupplierClient[] suppliers) =>
        new(
            suppliers,
            new MemoryCache(new MemoryCacheOptions()),
            Options.Create(new EquipmentOptions { SupplierTimeout = TimeSpan.FromMilliseconds(100) }),
            NullLogger<EquipmentService>.Instance);

    [Fact]
    public async Task Filters_And_SortsByDailyRateAscending()
    {
        var service = CreateService(
            FakeSupplier.Returning("A",
                new Equipment("1", "Expensive", 1500m, true, "A"),
                new Equipment("2", "Rented", 100m, false, "A"),
                new Equipment("3", "Mid", 850m, true, "A")),
            FakeSupplier.Returning("B",
                new Equipment("4", "Cheap", 600m, true, "B")));

        var response = await service.GetEquipmentAsync(available: true, maxDailyRate: 1000m);

        Assert.Equal(["Cheap", "Mid"], response.Items.Select(e => e.Name));
        Assert.Empty(response.Warnings);
    }

    [Fact]
    public async Task NoFilters_ReturnsEverything()
    {
        var service = CreateService(
            FakeSupplier.Returning("A",
                new Equipment("1", "Expensive", 1500m, true, "A"),
                new Equipment("2", "Rented", 100m, false, "A")));

        var response = await service.GetEquipmentAsync(available: null, maxDailyRate: null);

        Assert.Equal(2, response.Items.Count);
    }

    [Fact]
    public async Task OneSupplierFails_ReturnsOthersWithWarning()
    {
        var service = CreateService(
            FakeSupplier.Returning("A", new Equipment("1", "Excavator", 850m, true, "A")),
            FakeSupplier.Failing("B"));

        var response = await service.GetEquipmentAsync(null, null);

        var item = Assert.Single(response.Items);
        Assert.Equal("Excavator", item.Name);
        var warning = Assert.Single(response.Warnings);
        Assert.Contains("B", warning);
    }

    [Fact]
    public async Task SlowSupplier_TimesOutWithWarning()
    {
        var service = CreateService(
            FakeSupplier.Returning("A", new Equipment("1", "Excavator", 850m, true, "A")),
            FakeSupplier.Slow("B"));

        var response = await service.GetEquipmentAsync(null, null);

        Assert.Single(response.Items);
        Assert.Contains("timed out", Assert.Single(response.Warnings));
    }

    [Fact]
    public async Task SuccessfulResults_AreCached()
    {
        var supplier = FakeSupplier.Returning("A", new Equipment("1", "Excavator", 850m, true, "A"));
        var service = CreateService(supplier);

        await service.GetEquipmentAsync(null, null);
        await service.GetEquipmentAsync(null, null);

        Assert.Equal(1, supplier.Calls);
    }

    [Fact]
    public async Task Failures_AreNotCached()
    {
        var supplier = FakeSupplier.Failing("B");
        var service = CreateService(supplier);

        await service.GetEquipmentAsync(null, null);
        await service.GetEquipmentAsync(null, null);

        Assert.Equal(2, supplier.Calls);
    }

    private class FakeSupplier(string name, Func<Task<IReadOnlyList<Equipment>>> fetch) : ISupplierClient
    {
        public int Calls { get; private set; }

        public string Name => name;

        public Task<IReadOnlyList<Equipment>> GetEquipmentAsync(CancellationToken ct = default)
        {
            Calls++;
            return fetch();
        }

        public static FakeSupplier Returning(string name, params Equipment[] items) =>
            new(name, () => Task.FromResult<IReadOnlyList<Equipment>>(items));

        public static FakeSupplier Failing(string name) =>
            new(name, () => throw new HttpRequestException("Supplier is down"));

        public static FakeSupplier Slow(string name) =>
            new(name, async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(5));
                return Array.Empty<Equipment>();
            });
    }
}
