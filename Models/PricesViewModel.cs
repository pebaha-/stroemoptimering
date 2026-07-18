namespace StromligningApp.Models;

public sealed class PricesViewModel
{
    public IReadOnlyList<ElectricityPrice> Prices { get; init; } = [];

    public OptimalPeriodModel? CheapestPeriod { get; init; }

    public IReadOnlyList<OptimalPeriodModel> OptimalPeriods { get; init; } = [];
}
