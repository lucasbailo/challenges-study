using EquipmentAggregator.Api.Models;

namespace EquipmentAggregator.Api.Suppliers;

public interface ISupplierClient
{
    string Name { get; }
    Task<IReadOnlyList<Equipment>> GetEquipmentAsync(CancellationToken ct = default);
}
