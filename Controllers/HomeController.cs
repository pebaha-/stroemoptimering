using Microsoft.AspNetCore.Mvc;
using StromligningApp.Models;
using StromligningApp.Services;
using System.Diagnostics;

namespace StromligningApp.Controllers;

public class HomeController(StromligningService service) : Controller
{
    public async Task<IActionResult> Index(int hoursBack = 6)
    {
        var cutoff = DateTime.Now.AddHours(-hoursBack);

        var prices = await service.GetPricesAsync(cutoff);

        return View(new PricesViewModel
        {
            Prices = prices
        });
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
