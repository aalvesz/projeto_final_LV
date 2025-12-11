using projeto_final_LV.Models;

namespace projeto_final_LV.Repositories;

public interface IMovieRepository
{
    Task<int> CreateAsync(Movie f, CancellationToken ct = default);
    Task UpdateAsync(Movie f, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);

    Task<Movie?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Movie?> GetByTmdbIdAsync(int tmdbId, CancellationToken ct = default);
    Task<IReadOnlyList<Movie>> ListAsync(CancellationToken ct = default);
}
