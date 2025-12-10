using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using projeto_final_LV.Models.Options;
using projeto_final_LV.Models.Tmdb;

namespace projeto_final_LV.Services.Tmdb;

public sealed class TmdbApiService : ITmdbApiService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _http;
    private readonly IMemoryCache _cache;
    private readonly ILogger<TmdbApiService> _logger;
    private readonly TmdbOptions _opt;

    public TmdbApiService(HttpClient http, IMemoryCache cache, ILogger<TmdbApiService> logger, IOptions<TmdbOptions> options)
    {
        _http = http;
        _cache = cache;
        _logger = logger;
        _opt = options.Value;
    }

    public Task<TmdbPagedResponse<TmdbMovieSearchResultDto>> SearchMoviesAsync(string query, int page, CancellationToken ct = default)
    {
        query = (query ?? string.Empty).Trim();
        page = page < 1 ? 1 : page;

        var cacheKey = $"tmdb:search:{_opt.Language}:{page}:{query.ToLowerInvariant()}";

        return _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);

            var url =
                $"search/movie?query={Uri.EscapeDataString(query)}" +
                $"&page={page}" +
                $"&language={Uri.EscapeDataString(_opt.Language)}" +
                $"&include_adult=false";

            using var req = new HttpRequestMessage(HttpMethod.Get, url);

            if (!string.IsNullOrWhiteSpace(_opt.BearerToken))
            {
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _opt.BearerToken);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(_opt.ApiKey))
                    throw new InvalidOperationException("Configure Tmdb:BearerToken ou Tmdb:ApiKey nos secrets.");

                req.RequestUri = new Uri($"{url}&api_key={Uri.EscapeDataString(_opt.ApiKey)}", UriKind.Relative);
            }

            req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var at = DateTimeOffset.UtcNow;
            HttpResponseMessage res;

            try
            {
                res = await _http.SendAsync(req, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TMDb failed | endpoint={Endpoint} | q={Query} | page={Page} | at={At}", "search/movie", query, page, at);
                throw;
            }

            _logger.LogInformation("TMDb call | endpoint={Endpoint} | q={Query} | page={Page} | status={Status} | at={At}",
                "search/movie", query, page, (int)res.StatusCode, at);

            if (!res.IsSuccessStatusCode)
            {
                var body = await res.Content.ReadAsStringAsync(ct);
                _logger.LogError("TMDb error body: {Body}", body);
                throw new HttpRequestException($"TMDb retornou {(int)res.StatusCode}");
            }

            await using var stream = await res.Content.ReadAsStreamAsync(ct);
            var data = await JsonSerializer.DeserializeAsync<TmdbPagedResponse<TmdbMovieSearchResultDto>>(stream, JsonOptions, ct);
            return data ?? new TmdbPagedResponse<TmdbMovieSearchResultDto>();
        })!;
    }
}
