using Microsoft.AspNetCore.Mvc;

namespace projeto_final_LV.Controllers;

public sealed class FilmesController : Controller
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ImportFromTmdb([FromForm] int tmdbId)
    {
        return Ok(new { message = "Stub RF03 OK", tmdbId });
    }
}
