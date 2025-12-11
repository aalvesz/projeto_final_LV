using Microsoft.AspNetCore.Mvc;
using projeto_final_LV.Models.Tmdb;
using projeto_final_LV.Models.ViewModels;
using projeto_final_LV.Services.Tmdb;

namespace projeto_final_LV.Controllers;

public sealed class TmdbController : AppController
{
    private readonly ITmdbApiService _tmdb;

    public TmdbController(ITmdbApiService tmdb)
    {
        _tmdb = tmdb;
    }

    [HttpGet]
    public IActionResult Index()
    {
        // Redireciona para a busca
        return RedirectToAction(nameof(Search));
    }

    [HttpGet]
    public async Task<IActionResult> Search(string? query, int page = 1, CancellationToken ct = default)
    {
        query ??= string.Empty;
        ViewBag.Query = query;

        if (string.IsNullOrWhiteSpace(query))
        {
            // Primeira visita: não chama TMDb ainda
            return View(model: null);
        }

        var resp = await _tmdb.SearchMoviesAsync(query, page, ct);
        return View(resp);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id, CancellationToken ct = default)
    {
        var details = await _tmdb.GetMovieDetailsAsync(id, ct);
        var cfg = await _tmdb.GetConfigurationAsync(ct);
        var imgs = await _tmdb.GetMovieImagesAsync(id, ct);

        var baseUrl = cfg.Images.SecureBaseUrl.TrimEnd('/');
        var size = cfg.Images.PosterSizes.Contains("w500")
            ? "w500"
            : (cfg.Images.PosterSizes.FirstOrDefault() ?? "original");

        var posterBase = $"{baseUrl}/{size}";

        var posterUrl = string.IsNullOrWhiteSpace(details.PosterPath)
            ? null
            : $"{posterBase}{details.PosterPath}";

        var extras = imgs.Posters
            .Select(p => $"{posterBase}{p.FilePath}")
            .Distinct()
            .Take(6)
            .ToList();

        var vm = new TmdbDetailsVm
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
