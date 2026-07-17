namespace StromligningApp.Models;

public sealed class OptimalPeriodModel
{
    public DateTimeOffset StartTime { get; init; }

    public DateTimeOffset EndTime { get; init; }

    public decimal AveragePricePerKwh { get; init; }

    public DateTimeOffset LocalStartTime =>
        TimeZoneInfo.ConvertTime(
            StartTime,
            DanishTimeZone);

    public DateTimeOffset LocalEndTime =>
        TimeZoneInfo.ConvertTime(
            EndTime,
            DanishTimeZone);

    private static readonly TimeZoneInfo DanishTimeZone =
        TimeZoneInfo.FindSystemTimeZoneById("Europe/Copenhagen");
}
