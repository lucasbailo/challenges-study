namespace EquipmentAggregator.Api.Models;

public record Equipment(string Id, string Name, decimal DailyRate, bool IsAvailable, string Supplier);
