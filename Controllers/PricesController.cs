using Microsoft.AspNetCore.Mvc;
using StromligningApp.Services;

namespace StromligningApp.Controllers;

public class PricesController(StromligningService service, PriceOptimizationService optimizer) : Controller
{
    public async Task<IActionResult> Index()
    {
        var prices = await service.GetPricesAsync();

        return View(prices);
    }

    [HttpGet]
    public async Task<IActionResult> OptimalPeriods(int minutes)
    {
        var prices = await service.GetPricesAsync();

        var periods = optimizer.FindOptimalPeriods(prices, TimeSpan.FromMinutes(minutes));

        return PartialView("_OptimalPeriods", periods);
    }
}
