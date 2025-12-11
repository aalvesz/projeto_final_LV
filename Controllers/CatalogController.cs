using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using projeto_final_LV.Models;

public class CatalogController : Controller
{
    public IActionResult Catalog()
    {
        return View();
    }
}