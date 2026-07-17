using StromligningApp.Models;

namespace StromligningApp.Services;

public sealed class PriceOptimizationService
{
    public IReadOnlyList<OptimalPeriodModel> FindOptimalPeriods(IReadOnlyList<ElectricityPrice> prices, TimeSpan duration, int maxResults = 10)
    {
        if (prices.Count == 0)
        {
            return [];
        }

        prices = [.. prices.OrderBy(x => x.StartTime)];

        var interval = prices[0].EndTime - prices[0].StartTime;

        if (interval <= TimeSpan.Zero)
        {
            throw new InvalidOperationException("Invalid interval length.");
        }

        ArgumentOutOfRangeException.ThrowIfLessThan(duration, interval);

        foreach (var price in prices)
        {
            if (price.EndTime - price.StartTime != interval)
            {
                throw new InvalidOperationException("All price intervals must have identical duration.");
            }
        }

        var windowSize = (int)(duration.Ticks / interval.Ticks);

        if (windowSize == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(duration));
        }

        if (windowSize > prices.Count)
        {
            return [];
        }

        var results = new List<OptimalPeriodModel>();

        decimal runningSum = 0;

        // Initial window
        for (int i = 0; i < windowSize; i++)
        {
            runningSum += prices[i].PricePerKwh;
        }

        AddResult(0);

        // Slide the window
        for (int start = 1; start <= prices.Count - windowSize; start++)
        {
            runningSum -= prices[start - 1].PricePerKwh;
            runningSum += prices[start + windowSize - 1].PricePerKwh;

            AddResult(start);
        }

        return [.. results
            .OrderBy(x => x.AveragePricePerKwh)
            .Take(maxResults)];

        void AddResult(int startIndex)
        {
            var start = prices[startIndex];
            var end = prices[startIndex + windowSize - 1];

            results.Add(new OptimalPeriodModel
            {
                StartTime = start.StartTime,
                EndTime = end.EndTime,
                AveragePricePerKwh = runningSum / windowSize
            });
        }
    }
}