using EquipmentAggregator.Api.Models;
using EquipmentAggregator.Api.Suppliers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace EquipmentAggregator.Api.Services;

public class EquipmentService
{
    private readonly IEnumerable<ISupplierClient> _suppliers;
    private readonly IMemoryCache _cache;
    private readonly EquipmentOptions _options;
    private readonly ILogger<EquipmentService> _logger;

    public EquipmentService(
        IEnumerable<ISupplierClient> suppliers,
        IMemoryCache cache,
        IOptions<EquipmentOptions> options,
        ILogger<EquipmentService> logger)
    {
        _suppliers = suppliers;
        _cache = cache;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<EquipmentResponse> GetEquipmentAsync(bool? available, decimal? maxDailyRate, CancellationToken ct = default)
    {
        // Chama todos os fornecedores em paralelo
        var results = await Task.WhenAll(_suppliers.Select(s => FetchAsync(s, ct)));

        var items = results
            .SelectMany(r => r.Items)
            .Where(e => available is null || e.IsAvailable == available)
            .Where(e => maxDailyRate is null || e.DailyRate <= maxDailyRate)
            .OrderBy(e => e.DailyRate)
            .ToList();

        var warnings = results
            .Where(r => r.Warning is not null)
            .Select(r => r.Warning!)
            .ToList();

        return new EquipmentResponse(items, warnings);
    }

    // Nunca deixa a falha de um fornecedor derrubar a resposta: devolve itens OU um warning
    private async Task<SupplierResult> FetchAsync(ISupplierClient supplier, CancellationToken ct)
    {
        var cacheKey = $"supplier:{supplier.Name}";

        if (_cache.TryGetValue(cacheKey, out IReadOnlyList<Equipment>? cached) && cached is not null)
            return new SupplierResult(cached, null);

        try
        {
            var items = await supplier.GetEquipmentAsync(ct).WaitAsync(_options.SupplierTimeout, ct);

            // Só cacheia sucesso, para uma falha não ficar "presa" por 1 minuto
            _cache.Set(cacheKey, items, _options.CacheDuration);

            return new SupplierResult(items, null);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            // Quem chamou a API cancelou a requisição: não é falha do fornecedor
            throw;
        }
        catch (TimeoutException)
        {
            _logger.LogWarning("Supplier {Supplier} timed out after {Timeout}", supplier.Name, _options.SupplierTimeout);
            return new SupplierResult([], $"Supplier {supplier.Name} timed out.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Supplier {Supplier} failed", supplier.Name);
            return new SupplierResult([], $"Supplier {supplier.Name} is unavailable.");
        }
    }

    private record SupplierResult(IReadOnlyList<Equipment> Items, string? Warning);
}
