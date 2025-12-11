using Microsoft.AspNetCore.Mvc;

namespace projeto_final_LV.Controllers;

public sealed class HomeController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DefinirCidade(string? cidade)
    {
        cidade ??= "";

        Response.Cookies.Append(
            "CidadeSelecionada",         
            cidade,
            new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddDays(30),
                IsEssential = true
            });

        // Volta para a mesma página em que o usuário estava
        var referer = Request.Headers.Referer.ToString();
        if (!string.IsNullOrWhiteSpace(referer))
            return Redirect(referer);

        // fallback: se não tiver Referer, vai pra Home
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Privacy()
    {
        return View();
    }
}
