using Microsoft.AspNetCore.Mvc;
using projeto_final_LV.Services.Tmdb;

namespace projeto_final_LV.Controllers;

public sealed class TmdbController : Controller
{
    private readonly ITmdbApiService _tmdb;

    public TmdbController(ITmdbApiService tmdb) => _tmdb = tmdb;

    [HttpGet]
    public async Task<IActionResult> Search(string? query, int page = 1, CancellationToken ct = default)
    {
        query = (query ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(query))
            return View(null);

        var resp = await _tmdb.SearchMoviesAsync(query, page, ct);
        ViewBag.Query = query;
        return View(resp);
    }
}
