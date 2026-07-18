using Microsoft.AspNetCore.Mvc;
using StromligningApp.Models;
using StromligningApp.Services;

namespace StromligningApp.Controllers;

public class PricesController(StromligningService service, PriceOptimizationService optimizer) : Controller
{
    public async Task<IActionResult> Index()
    {
        var prices = await service.GetPricesAsync();

        var model = new PricesViewModel
        {
            Prices = prices
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> OptimalPeriods(int minutes)
    {
        var prices = await service.GetPricesAsync();

        var periods = optimizer.FindOptimalPeriods(prices, TimeSpan.FromMinutes(minutes));

        return PartialView("_OptimalPeriods", periods);
    }
}
