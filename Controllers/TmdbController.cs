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

    [HttpGet]
    public async Task<IActionResult> Details(int id, CancellationToken ct = default)
    {
        var details = await _tmdb.GetMovieDetailsAsync(id, ct);
        var cfg = await _tmdb.GetConfigurationAsync(ct);
        var imgs = await _tmdb.GetMovieImagesAsync(id, ct);

        var baseUrl = cfg.Images.SecureBaseUrl.TrimEnd('/');
        var size = cfg.Images.PosterSizes.Contains("w500") ? "w500" : (cfg.Images.PosterSizes.FirstOrDefault() ?? "original");
        var posterBase = $"{baseUrl}/{size}";

        var posterUrl = string.IsNullOrWhiteSpace(details.PosterPath) ? null : $"{posterBase}{details.PosterPath}";

        var extras = imgs.Posters
            .Select(p => $"{posterBase}{p.FilePath}")
            .Distinct()
            .Take(6)
            .ToList();

        var vm = new projeto_final_LV.Models.ViewModels.TmdbDetailsVm
        {
            TmdbId = details.Id,
            Titulo = details.Title ?? "(sem título)",
            TituloOriginal = details.OriginalTitle,
            Sinopse = details.Overview,
            DataLancamento = details.ReleaseDate,
            Generos = string.Join(", ", details.Genres.Select(g => g.Name).Where(n => !string.IsNullOrWhiteSpace(n))),
            Duracao = details.Runtime,
            NotaMedia = details.VoteAverage,
            Lingua = details.OriginalLanguage,
            PosterUrl = posterUrl,
            PosterUrlsExtras = extras
        };

        return View(vm);
    }

}
