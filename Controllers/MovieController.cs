using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using projeto_final_LV.Models;

namespace projeto_final_LV.Controllers;

public class MovieController : Controller
{
    public IActionResult Movie()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}