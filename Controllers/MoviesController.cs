using Microsoft.AspNetCore.Mvc;
using projeto_final_LV.Models;
using projeto_final_LV.Repositories;
using projeto_final_LV.Services.Tmdb;

namespace projeto_final_LV.Controllers;

// Rota base: /Movies/...
[Route("[controller]")]
public sealed class MoviesController : Controller
{
    private readonly ITmdbApiService _tmdb;
    private readonly IMovieRepository _repo;

    public MoviesController(ITmdbApiService tmdb, IMovieRepository repo)
    {
        _tmdb = tmdb;
        _repo = repo;
    }

    // GET /Movies
    // Catálogo local
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var list = await _repo.ListAsync(ct);
        return View(list);
    }

    // POST /Movies/ImportFromTmdb
    [HttpPost("ImportFromTmdb")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ImportFromTmdb(
        [FromForm] int tmdbId,
        [FromForm] string? referenceCity,
        [FromForm] double? latitude,
        [FromForm] double? longitude,
        CancellationToken ct)
    {
        // se já estiver importado, só devolve o id local
        var existing = await _repo.GetByTmdbIdAsync(tmdbId, ct);
        if (existing is not null)
        {
            return Ok(new
            {
                message = "Já importado",
                id = existing.Id,
                tmdbId
            });
        }

        // busca detalhes no TMDb
        var d = await _tmdb.GetMovieDetailsAsync(tmdbId, ct);

        var movie = new Movie
        {
            TmdbId = d.Id,
            Titulo = d.Title ?? "(sem título)",
            TituloOriginal = d.OriginalTitle,
            Sinopse = d.Overview,
            DataLancamento = DateOnly.TryParse(d.ReleaseDate, out var dt) ? dt : null,
            Genero = string.Join(", ", d.Genres.Select(g => g.Name).Where(x => !string.IsNullOrWhiteSpace(x))),
            PosterPath = d.PosterPath,
            Lingua = d.OriginalLanguage,
            Duracao = d.Runtime,
            NotaMedia = d.VoteAverage,
            ElencoPrincipal = null,
            CidadeReferencia = string.IsNullOrWhiteSpace(referenceCity) ? null : referenceCity,
            Latitude = latitude,
            Longitude = longitude,
            DataCriacao = DateTimeOffset.UtcNow,
            DataAtualizacao = DateTimeOffset.UtcNow
        };

        var id = await _repo.CreateAsync(movie, ct);

        return Ok(new
        {
            message = "Importado com sucesso",
            id,
            tmdbId,
            titulo = movie.Titulo
        });
    }
}
