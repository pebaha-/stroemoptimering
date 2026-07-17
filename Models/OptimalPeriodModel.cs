namespace StromligningApp.Models
{
    public sealed class OptimalPeriodModel
    {
        public DateTimeOffset StartTime { get; init; }

        public DateTimeOffset EndTime { get; init; }

        public decimal AveragePricePerKwh { get; init; }
    }
}
