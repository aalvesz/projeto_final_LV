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

    public TmdbApiService(
        HttpClient http,
        IMemoryCache cache,
        ILogger<TmdbApiService> logger,
        IOptions<TmdbOptions> options)
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

            using var req = new HttpRequestMessage(HttpMethod.Get, WithAuth(url));
            req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return await SendAsync<TmdbPagedResponse<TmdbMovieSearchResultDto>>(
                req,
                endpoint: "search/movie",
                parameters: new { query, page, _opt.Language },
                ct
            );
        })!;
    }

    public Task<TmdbMovieDetailsDto> GetMovieDetailsAsync(int tmdbId, CancellationToken ct = default)
    {
        var cacheKey = $"tmdb:details:{_opt.Language}:{tmdbId}";

        return _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);

            var url =
                $"movie/{tmdbId}?language={Uri.EscapeDataString(_opt.Language)}" +
                $"&append_to_response=credits";

            using var req = new HttpRequestMessage(HttpMethod.Get, WithAuth(url));
            req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return await SendAsync<TmdbMovieDetailsDto>(
                req,
                endpoint: "movie/{tmdbId}",
                parameters: new { tmdbId, _opt.Language },
                ct
            );
        })!;
    }


    public Task<TmdbMovieImagesDto> GetMovieImagesAsync(int tmdbId, CancellationToken ct = default)
    {
        var cacheKey = $"tmdb:images:{tmdbId}";

        return _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);

            var url = $"movie/{tmdbId}/images";

            using var req = new HttpRequestMessage(HttpMethod.Get, WithAuth(url));
            req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return await SendAsync<TmdbMovieImagesDto>(
                req,
                endpoint: "movie/{id}/images",
                parameters: new { tmdbId },
                ct
            );
        })!;
    }

    public Task<TmdbConfigurationDto> GetConfigurationAsync(CancellationToken ct = default)
    {
        const string cacheKey = "tmdb:configuration";

        return _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(6);

            using var req = new HttpRequestMessage(HttpMethod.Get, WithAuth("configuration"));
            req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return await SendAsync<TmdbConfigurationDto>(
                req,
                endpoint: "configuration",
                parameters: null,
                ct
            );
        })!;
    }

    private string WithAuth(string relativeUrl)
    {
        if (!string.IsNullOrWhiteSpace(_opt.BearerToken))
            return relativeUrl;

        if (string.IsNullOrWhiteSpace(_opt.ApiKey))
            throw new InvalidOperationException("Configure Tmdb:BearerToken ou Tmdb:ApiKey nos secrets.");

        return relativeUrl.Contains('?')
            ? $"{relativeUrl}&api_key={Uri.EscapeDataString(_opt.ApiKey)}"
            : $"{relativeUrl}?api_key={Uri.EscapeDataString(_opt.ApiKey)}";
    }

    private async Task<T> SendAsync<T>(HttpRequestMessage req, string endpoint, object? parameters, CancellationToken ct)
    {
        var at = DateTimeOffset.UtcNow;
        HttpResponseMessage res;

        try
        {
            if (!string.IsNullOrWhiteSpace(_opt.BearerToken) && req.Headers.Authorization is null)
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _opt.BearerToken);

            res = await _http.SendAsync(req, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "TMDb failed | endpoint={Endpoint} | params={Params} | at={At}", endpoint, parameters, at);
            throw;
        }

        _logger.LogInformation("TMDb call | endpoint={Endpoint} | params={Params} | status={Status} | at={At}",
            endpoint, parameters, (int)res.StatusCode, at);

        if (!res.IsSuccessStatusCode)
        {
            var body = await res.Content.ReadAsStringAsync(ct);
            _logger.LogError("TMDb error body | endpoint={Endpoint} | body={Body}", endpoint, body);
            throw new HttpRequestException($"TMDb retornou {(int)res.StatusCode}");
        }

        await using var stream = await res.Content.ReadAsStreamAsync(ct);
        var data = await JsonSerializer.DeserializeAsync<T>(stream, JsonOptions, ct);
        return data ?? throw new InvalidOperationException($"TMDb payload vazio: {endpoint}");
    }
}
