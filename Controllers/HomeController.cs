using Microsoft.AspNetCore.Mvc;
using StromligningApp.Models;
using StromligningApp.Services;
using System.Diagnostics;

namespace StromligningApp.Controllers;

public class HomeController(StromligningService service) : Controller
{
    public async Task<IActionResult> Index()
    {
        var model = new PricesViewModel
        {
            Prices = await service.GetPricesAsync()
        };

        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
