using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using projeto_final_LV.Models.Weather;

namespace projeto_final_LV.Services.Weather;

public sealed class WeatherApiService : IWeatherApiService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _http;
    private readonly IMemoryCache _cache;
    private readonly ILogger<WeatherApiService> _logger;

    public WeatherApiService(HttpClient http, IMemoryCache cache, ILogger<WeatherApiService> logger)
    {
        _http = http;
        _cache = cache;
        _logger = logger;
    }

    public Task<OpenMeteoForecastDto> GetDailyForecastAsync(double lat, double lon, CancellationToken ct = default)
    {
        var latKey = Math.Round(lat, 4);
        var lonKey = Math.Round(lon, 4);
        var cacheKey = $"weather:open-meteo:{latKey}:{lonKey}";

        return _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);

            var latStr = lat.ToString(CultureInfo.InvariantCulture);
            var lonStr = lon.ToString(CultureInfo.InvariantCulture);

            var url =
                $"forecast?latitude={latStr}" +
                $"&longitude={lonStr}" +
                $"&daily=temperature_2m_max,temperature_2m_min" +
                $"&timezone=auto";

            var at = DateTimeOffset.UtcNow;
            HttpResponseMessage res;

            try
            {
                res = await _http.GetAsync(url, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Open-Meteo failed | url={Url} | at={At}", url, at);
                throw;
            }

            _logger.LogInformation("Open-Meteo call | url={Url} | status={Status} | at={At}", url, (int)res.StatusCode, at);

            if (!res.IsSuccessStatusCode)
            {
                var body = await res.Content.ReadAsStringAsync(ct);
                _logger.LogError("Open-Meteo error body | body={Body}", body);
                throw new HttpRequestException($"Open-Meteo retornou {(int)res.StatusCode}");
            }

            await using var stream = await res.Content.ReadAsStreamAsync(ct);
            var data = await JsonSerializer.DeserializeAsync<OpenMeteoForecastDto>(stream, JsonOptions, ct);

            return data ?? new OpenMeteoForecastDto();
        })!;
    }
}
