using System.Net;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using StromligningApp.Services;

namespace StromligningApp.Tests;

[TestClass]
public sealed class StromligningServiceTests
{
    [TestMethod]
    public async Task GetPricesAsync_MapsFifteenMinuteResolution()
    {
        var start = new DateTimeOffset(2026, 7, 22, 10, 0, 0, TimeSpan.FromHours(2));
        var (service, _) = CreateService(JsonPrice(start, 1.25m, "15m"));

        var price = AssertSingle(await service.GetPricesAsync(start.AddHours(-1)));

        Assert.AreEqual(start, price.StartTime);
        Assert.AreEqual(start.AddMinutes(15), price.EndTime);
        Assert.AreEqual(1.25m, price.PricePerKwh);
    }

    [TestMethod]
    public async Task GetPricesAsync_MapsHourlyResolution()
    {
        var start = new DateTimeOffset(2026, 7, 22, 10, 0, 0, TimeSpan.FromHours(2));
        var (service, _) = CreateService(JsonPrice(start, 2.10m, "1h"));

        var price = AssertSingle(await service.GetPricesAsync(start.AddHours(-1)));

        Assert.AreEqual(start.AddHours(1), price.EndTime);
    }

    [TestMethod]
    public async Task GetPricesAsync_PreservesNegativePricesAndOffset()
    {
        var start = new DateTimeOffset(2026, 1, 15, 8, 0, 0, TimeSpan.FromHours(1));
        var (service, _) = CreateService(JsonPrice(start, -0.42m, "15m"));

        var price = AssertSingle(await service.GetPricesAsync(start.AddHours(-1)));

        Assert.AreEqual(-0.42m, price.PricePerKwh);
        Assert.AreEqual(start, price.StartTime);
        Assert.AreEqual(TimeSpan.FromHours(1), price.StartTime.Offset);
    }

    [TestMethod]
    public async Task GetPricesAsync_ThrowsForUnknownResolution()
    {
        var start = new DateTimeOffset(2026, 7, 22, 10, 0, 0, TimeSpan.FromHours(2));
        var (service, _) = CreateService(JsonPrice(start, 1.00m, "30m"));

        var exception = await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            () => service.GetPricesAsync(start.AddHours(-1)));

        StringAssert.Contains(exception.Message, "30m");
    }

    [TestMethod]
    public async Task GetPricesAsync_ExcludesPricesEndingAtOrBeforeCutoff()
    {
        var start = new DateTimeOffset(2026, 7, 22, 10, 0, 0, TimeSpan.FromHours(2));
        var json = JsonPrices(
            (start, 1.00m, "15m"),
            (start.AddMinutes(15), 2.00m, "15m"),
            (start.AddMinutes(30), 3.00m, "15m"));
        var (service, _) = CreateService(json);

        var prices = await service.GetPricesAsync(start.AddMinutes(30));

        Assert.HasCount(1, prices);
        Assert.AreEqual(start.AddMinutes(30), prices[0].StartTime);
    }

    [TestMethod]
    public async Task GetPricesAsync_ReturnsPricesEndingAfterCutoffAcrossOffsets()
    {
        var start = new DateTimeOffset(2026, 7, 22, 10, 0, 0, TimeSpan.FromHours(2));
        var equivalentUtcCutoff = new DateTimeOffset(2026, 7, 22, 8, 10, 0, TimeSpan.Zero);
        var (service, _) = CreateService(JsonPrice(start, 1.00m, "15m"));

        var prices = await service.GetPricesAsync(equivalentUtcCutoff);

        Assert.HasCount(1, prices);
    }

    [TestMethod]
    public async Task GetPricesAsync_UsesCachedApiResponse()
    {
        var start = new DateTimeOffset(2026, 7, 22, 10, 0, 0, TimeSpan.FromHours(2));
        var (service, handler) = CreateService(JsonPrice(start, 1.00m, "15m"));

        await service.GetPricesAsync(start.AddHours(-1));
        await service.GetPricesAsync(start.AddHours(-1));

        Assert.AreEqual(1, handler.RequestCount);
    }

    [TestMethod]
    public async Task GetPricesAsync_AppliesDifferentCutoffsToCachedResponse()
    {
        var start = new DateTimeOffset(2026, 7, 22, 10, 0, 0, TimeSpan.FromHours(2));
        var json = JsonPrices(
            (start, 1.00m, "15m"),
            (start.AddMinutes(15), 2.00m, "15m"));
        var (service, handler) = CreateService(json);

        var allPrices = await service.GetPricesAsync(start.AddMinutes(-1));
        var lastPrice = await service.GetPricesAsync(start.AddMinutes(15));

        Assert.HasCount(2, allPrices);
        Assert.HasCount(1, lastPrice);
        Assert.AreEqual(start.AddMinutes(15), lastPrice[0].StartTime);
        Assert.AreEqual(1, handler.RequestCount);
    }

    [TestMethod]
    public async Task GetPricesAsync_ReturnsEmptyListWhenApiReturnsNull()
    {
        var (service, handler) = CreateService("null");

        var prices = await service.GetPricesAsync(DateTimeOffset.MinValue);

        Assert.IsEmpty(prices);
        Assert.AreEqual(1, handler.RequestCount);
    }

    [TestMethod]
    public async Task GetPricesAsync_ReturnsEmptyListWhenCacheContainsNull()
    {
        var handler = new StubHttpMessageHandler("[]");
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://stromligning.dk/")
        };
        var cache = new MemoryCache(new MemoryCacheOptions());
        cache.Set<IReadOnlyList<Models.ElectricityPrice>?>(
            "stromligning-prices",
            null);
        var service = new StromligningService(
            httpClient,
            cache,
            NullLogger<StromligningService>.Instance);

        var prices = await service.GetPricesAsync(DateTimeOffset.MinValue);

        Assert.IsEmpty(prices);
        Assert.AreEqual(0, handler.RequestCount);
    }

    private static (StromligningService Service, StubHttpMessageHandler Handler) CreateService(string json)
    {
        var handler = new StubHttpMessageHandler(json);
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://stromligning.dk/")
        };
        var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new StromligningService(
            httpClient,
            cache,
            NullLogger<StromligningService>.Instance);

        return (service, handler);
    }

    private static Models.ElectricityPrice AssertSingle(
        IReadOnlyList<Models.ElectricityPrice> prices)
    {
        Assert.HasCount(1, prices);
        return prices[0];
    }

    private static string JsonPrice(DateTimeOffset start, decimal price, string resolution) =>
        JsonPrices((start, price, resolution));

    private static string JsonPrices(
        params (DateTimeOffset Start, decimal Price, string Resolution)[] prices)
    {
        var entries = prices.Select(price =>
            $$"""{"date":"{{price.Start:O}}","price":{{price.Price.ToString(System.Globalization.CultureInfo.InvariantCulture)}},"resolution":"{{price.Resolution}}"}""");

        return $"[{string.Join(',', entries)}]";
    }

    private sealed class StubHttpMessageHandler(string responseJson) : HttpMessageHandler
    {
        public int RequestCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestCount++;

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
            });
        }
    }
}
