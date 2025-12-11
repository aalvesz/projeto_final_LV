using Microsoft.AspNetCore.Mvc;
using projeto_final_LV.Models;
using projeto_final_LV.Models.ViewModels;
using projeto_final_LV.Repositories;
using projeto_final_LV.Services.Tmdb;
using projeto_final_LV.Services.Weather;
using projeto_final_LV.Models.Weather;

namespace projeto_final_LV.Controllers;

[Route("[controller]")]
public sealed class MoviesController : AppController
{
    private readonly ITmdbApiService _tmdb;
    private readonly IMovieRepository _repo;
    private readonly IWeatherApiService _weather;

    public MoviesController(
        ITmdbApiService tmdb,
        IMovieRepository repo,
        IWeatherApiService weather)
    {
        _tmdb = tmdb;
        _repo = repo;
        _weather = weather;
    }

    private static DateOnly? ParseDateOnly(string? value)
{
    if (string.IsNullOrWhiteSpace(value))
        return null;

    return DateOnly.TryParse(value, out var d) ? d : null;
}


    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var list = await _repo.ListAsync(ct);
        return View(list);
    }

        // GET /Movies/Create
    [HttpGet("Create")]
    public IActionResult Create()
    {
        // formulário vazio
        return View(new Movie());
    }

    // POST /Movies/Create
    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Movie input, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return View(input);

        // Mapeia cidade -> lat/lon
        if (!string.IsNullOrWhiteSpace(input.CidadeReferencia) &&
            CityLocationHelper.TryGetCoordinates(input.CidadeReferencia, out var lat, out var lon))
        {
            input.Latitude = lat;
            input.Longitude = lon;
        }

        input.DataCriacao = DateTimeOffset.UtcNow;
        input.DataAtualizacao = DateTimeOffset.UtcNow;

        var id = await _repo.CreateAsync(input, ct);

        return RedirectToAction(nameof(Details), new { id });
    }


     [HttpGet("Details/{id:int}")]
    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        var movie = await _repo.GetByIdAsync(id, ct);
        if (movie is null)
            return NotFound();

        WeatherDailySummary? weather = null;

        if (movie.Latitude.HasValue && movie.Longitude.HasValue)
        {
            try
            {
                weather = await _weather.GetDailySummaryAsync(
                    movie.Latitude.Value,
                    movie.Longitude.Value,
                    ct);
            }
            catch (Exception ex)
            {
                // loga, mas não quebra a tela
                Console.WriteLine(ex);
            }
        }

        var vm = new MovieDetailsVm
        {
            Movie = movie,
            Weather = weather
        };

        return View(vm);
    }

        // GET /Movies/Edit/5
    [HttpGet("Edit/{id:int}")]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var movie = await _repo.GetByIdAsync(id, ct);
        if (movie is null)
            return NotFound();

        return View(movie);
    }

    // POST /Movies/Edit/5
    [HttpPost("Edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Movie input, CancellationToken ct)
    {
        if (id != input.Id)
            return BadRequest();

        if (!ModelState.IsValid)
            return View(input);

        var existing = await _repo.GetByIdAsync(id, ct);
        if (existing is null)
            return NotFound();

        // Atualiza apenas os campos que fazem sentido editar manualmente
        existing.Titulo = input.Titulo;
        existing.CidadeReferencia = input.CidadeReferencia;
        existing.Latitude = input.Latitude;
        existing.Longitude = input.Longitude;
        existing.Sinopse = input.Sinopse;
        existing.Genero = input.Genero;
        existing.Lingua = input.Lingua;
        existing.Duracao = input.Duracao;
        existing.NotaMedia = input.NotaMedia;
        existing.ElencoPrincipal = input.ElencoPrincipal;

        existing.Touch(); // atualiza DataAtualizacao

        await _repo.UpdateAsync(existing, ct);

        return RedirectToAction(nameof(Details), new { id = existing.Id });
    }

        // GET /Movies/Delete/5  (tela de confirmação)
    [HttpGet("Delete/{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var movie = await _repo.GetByIdAsync(id, ct);
        if (movie is null)
            return NotFound();

        return View(movie);
    }

    // POST /Movies/Delete/5
    [HttpPost("Delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmDelete(int id, CancellationToken ct)
    {
        var movie = await _repo.GetByIdAsync(id, ct);
        if (movie is null)
            return NotFound();

        await _repo.DeleteAsync(id, ct);

        return RedirectToAction(nameof(Index));
    }

    // POST /Movies/ImportFromTmdb
    [HttpPost("ImportFromTmdb")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ImportFromTmdb(
        int tmdbId,
        string? referenceCity,
        CancellationToken ct)
    {
        if (tmdbId <= 0)
            return BadRequest(new { ok = false, error = "tmdbId inválido." });

        var existing = await _repo.GetByTmdbIdAsync(tmdbId, ct);
        if (existing is not null)
        {
            return Json(new
            {
                ok = true,
                id = existing.Id,
                alreadyExists = true
            });
        }

        var details = await _tmdb.GetMovieDetailsAsync(tmdbId, ct);

        var genres = string.Join(", ",
            details.Genres
                .Select(g => g.Name)
                .Where(n => !string.IsNullOrWhiteSpace(n)));

        string? castNames = null;
        if (details.Credits?.Cast is { Count: > 0 })
        {
            castNames = string.Join(", ",
                details.Credits.Cast
                    .Where(c => !string.IsNullOrWhiteSpace(c.Name))
                    .Take(5)
                    .Select(c => c.Name));
        }

        double? latitude = null;
        double? longitude = null;

        if (!string.IsNullOrWhiteSpace(referenceCity) &&
            CityLocationHelper.TryGetCoordinates(referenceCity, out var lat, out var lon))
        {
            latitude = lat;
            longitude = lon;
        }

        var movie = new Movie
        {
            TmdbId = details.Id,
            Titulo = details.Title ?? "(sem título)",
            TituloOriginal = details.OriginalTitle,
            Sinopse = details.Overview,
            DataLancamento = ParseDateOnly(details.ReleaseDate),
            Genero = genres,
            PosterPath = details.PosterPath,
            Lingua = details.OriginalLanguage,
            Duracao = details.Runtime,
            NotaMedia = details.VoteAverage,
            ElencoPrincipal = castNames,
            CidadeReferencia = referenceCity,
            Latitude = latitude,
            Longitude = longitude,
            DataCriacao = DateTimeOffset.UtcNow,
            DataAtualizacao = DateTimeOffset.UtcNow
        };

        var id = await _repo.CreateAsync(movie, ct);

        return Json(new
        {
            ok = true,
            id,
            alreadyExists = false
        });
    }
}
