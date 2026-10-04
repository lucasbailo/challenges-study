namespace EquipmentAggregator.Api.Models;

public record EquipmentResponse(IReadOnlyList<Equipment> Items, IReadOnlyList<string> Warnings);
