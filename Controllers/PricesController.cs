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
    public async Task<IActionResult> OptimalPeriods(int minutes)
    {
        if (minutes is not (30 or 60 or 120))
        {
            return BadRequest();
        }

        return PartialView("_PriceResults", await CreateModelAsync(minutes));
    }

    private async Task<PricesViewModel> CreateModelAsync(int minutes)
    {
        var prices = await service.GetPricesAsync();
        var periods = optimizer.FindOptimalPeriods(
            prices,
            TimeSpan.FromMinutes(minutes));

        return new PricesViewModel
        {
            Prices = prices,
            CheapestPeriod = periods.FirstOrDefault(),
            OptimalPeriods = periods
        };
    }
}
