using Microsoft.AspNetCore.Mvc;
using StromligningApp.Services;

namespace StromligningApp.Controllers;

public class PricesController(StromligningService service) : Controller
{
    public async Task<IActionResult> Index()
    {
        var prices = await service.GetPricesAsync();

        return View(prices);
    }
}
