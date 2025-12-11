using Microsoft.AspNetCore.Mvc;
using projeto_final_LV.Models.ViewModels;
using projeto_final_LV.Services.Tmdb;
using projeto_final_LV.Services.Weather;

namespace projeto_final_LV.Controllers;

public sealed class TmdbController : Controller
{
    private readonly ITmdbApiService _tmdb;
    private readonly IWeatherApiService _weather;

    public TmdbController(ITmdbApiService tmdb, IWeatherApiService weather)
    {
        _tmdb = tmdb;
        _weather = weather;
    }

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
        var size = cfg.Images.PosterSizes.Contains("w500")
            ? "w500"
            : (cfg.Images.PosterSizes.FirstOrDefault() ?? "original");

        var posterBase = $"{baseUrl}/{size}";
        var posterUrl = string.IsNullOrWhiteSpace(details.PosterPath) ? null : $"{posterBase}{details.PosterPath}";

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
            PosterUrlsExtras = extras,
            Latitude = null,
            Longitude = null
        };

        if (vm.Latitude is null || vm.Longitude is null)
        {
            vm.Weather = new WeatherBlockVm { HasCoordinates = false };
            return View(vm);
        }

        try
        {
            var fc = await _weather.GetDailyForecastAsync(vm.Latitude.Value, vm.Longitude.Value, ct);

            var date = fc.Daily.Time.FirstOrDefault();
            var min = fc.Daily.TemperatureMin.Count > 0 ? fc.Daily.TemperatureMin[0] : (double?)null;
            var max = fc.Daily.TemperatureMax.Count > 0 ? fc.Daily.TemperatureMax[0] : (double?)null;

            vm.Weather = new WeatherBlockVm
            {
                HasCoordinates = true,
                Date = date,
                Min = min,
                Max = max,
                Message = date is null ? "Sem dados diários retornados." : null
            };
        }
        catch
        {
            vm.Weather = new WeatherBlockVm
            {
                HasCoordinates = true,
                Message = "Falha ao consultar a previsão do tempo."
            };
        }

        return View(vm);
    }
}
