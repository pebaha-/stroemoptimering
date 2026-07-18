using Microsoft.AspNetCore.Mvc;
using StromligningApp.Models;
using StromligningApp.Services;

namespace StromligningApp.Controllers;

public class PricesController(StromligningService service) : Controller
{
    public async Task<IActionResult> Index()
    {
        var model = new PricesViewModel
        {
            Prices = await service.GetPricesAsync()
        };

        return View(model);
    }
}
