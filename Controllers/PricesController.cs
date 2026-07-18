using Microsoft.AspNetCore.Mvc;
using StromligningApp.Models;
using StromligningApp.Services;

namespace StromligningApp.Controllers;

public class PricesController(StromligningService service, PriceOptimizationService optimizer) : Controller
{
    public async Task<IActionResult> Index()
    {
        return View(await CreateModelAsync(minutes: 120));
    }

    [HttpGet]
    public async Task<IActionResult> OptimalPeriods(int hours, int minutes)
    {
        var totalMinutes = hours * 60 + minutes;

        // Minimum 5 minutes, maximum 24 hours
        if (totalMinutes is < 5 or > 60 * 24)
        {
            return BadRequest();
        }

        return PartialView("_PriceResults", await CreateModelAsync(totalMinutes));
    }

    private async Task<PricesViewModel> CreateModelAsync(int minutes)
    {
        var prices = await service.GetPricesAsync();
        var periods = optimizer.FindOptimalPeriods(prices, TimeSpan.FromMinutes(minutes));

        return new PricesViewModel
        {
            Prices = prices,
            CheapestPeriod = periods.FirstOrDefault(),
            OptimalPeriods = periods
        };
    }
}
