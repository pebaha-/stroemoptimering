using Microsoft.Extensions.Caching.Memory;
using StromligningApp.Models;

namespace StromligningApp.Services;

public sealed class StromligningService(HttpClient httpClient, IMemoryCache cache, ILogger<StromligningService> logger)
{
    public async Task<IReadOnlyList<ElectricityPrice>> GetPricesAsync()
    {
        return await cache.GetOrCreateAsync(
            "stromligning-prices",
            async entry =>
            {
                logger.LogInformation("Refreshing Stromligning price cache.");
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);

                return await FetchPricesAsync();
            })
            ?? [];
    }

    private async Task<IReadOnlyList<ElectricityPrice>> FetchPricesAsync()
    {
        logger.LogInformation("Fetching prices from Stromligning API...");
        const string url =
            "api/prices" +
            "?productId=vindstoed_danskvind" +
            "&supplierId=konstant_c" +
            "&customerGroupId=c" +
            "&priceArea=dk1" +
            "&lean=true" +
            "&forecast=false";

        var data = await httpClient.GetFromJsonAsync<List<StromligningPriceDto>>(url) ?? [];
        logger.LogInformation("Fetched {Count} electricity prices.", data.Count);

        return [.. data.Select(Map)];
    }

    private static ElectricityPrice Map(StromligningPriceDto dto)
    {
        var duration = dto.Resolution switch
        {
            "15m" => TimeSpan.FromMinutes(15),
            "1h" => TimeSpan.FromHours(1),
            _ => throw new InvalidOperationException(
                $"Ukendt opløsning: {dto.Resolution}")
        };

        return new ElectricityPrice
        {
            StartTime = dto.Date,
            EndTime = dto.Date.Add(duration),
            PricePerKwh = dto.Price
        };
    }
}
