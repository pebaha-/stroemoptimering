using StromligningApp.Models;

namespace StromligningApp.Services;

public sealed class StromligningService(HttpClient httpClient)
{
    public async Task<IReadOnlyList<ElectricityPrice>> GetPricesAsync()
    {
        const string url =
            "api/prices" +
            "?productId=vindstoed_danskvind" +
            "&supplierId=konstant_c" +
            "&customerGroupId=c" +
            "&priceArea=dk1" +
            "&lean=true" +
            "&forecast=false";

        var data = await httpClient
            .GetFromJsonAsync<List<StromligningPriceDto>>(url)
            ?? [];

        return data.Select(Map).ToList();
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