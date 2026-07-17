using StromligningApp.Models;

public sealed class StromligningService(HttpClient httpClient)
{
    public async Task<IReadOnlyList<ElectricityPrice>> GetPricesAsync()
    {
        return await httpClient.GetFromJsonAsync<List<ElectricityPrice>>("api/prices") ?? [];
    }
}
