namespace StromligningApp.Models
{
    public sealed class ElectricityPrice
    {
        public DateTimeOffset StartTime { get; init; }

        public DateTimeOffset EndTime { get; init; }

        public decimal PricePerKwh { get; init; }
    }
}
