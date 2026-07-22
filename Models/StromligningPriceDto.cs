namespace StromligningApp.Models;

public sealed class StromligningPriceDto
{
    public DateTimeOffset Date { get; init; }

    public decimal Price { get; init; }

    public string Resolution { get; init; } = "";
}
