namespace StromligningApp.Models;

public sealed class PricesViewModel
{
    public IReadOnlyList<ElectricityPrice> Prices { get; init; } = [];
}
