using projeto_final_LV.Models.Tmdb;

namespace projeto_final_LV.Services.Tmdb;

public interface ITmdbApiService
{
    Task<TmdbPagedResponse<TmdbMovieSearchResultDto>> SearchMoviesAsync(string query, int page, CancellationToken ct = default);
}
