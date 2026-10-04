namespace EquipmentAggregator.Api.Services;

public class EquipmentOptions
{
    public const string SectionName = "Equipment";

    public TimeSpan SupplierTimeout { get; set; } = TimeSpan.FromSeconds(5);
    public TimeSpan CacheDuration { get; set; } = TimeSpan.FromMinutes(1);
}
