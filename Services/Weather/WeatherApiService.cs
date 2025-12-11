using System.Globalization;
using System.Net.Http.Headers;
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

    public Task<WeatherDailySummary?> GetDailySummaryAsync(
        double latitude,
        double longitude,
        CancellationToken ct = default)
    {
        var latStr = latitude.ToString(CultureInfo.InvariantCulture);
        var lonStr = longitude.ToString(CultureInfo.InvariantCulture);

        var cacheKey = $"weather:{latStr}:{lonStr}";

        return _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);

            var url =
                $"forecast?latitude={latStr}" +
                $"&longitude={lonStr}" +
                $"&daily=temperature_2m_max,temperature_2m_min" +
                $"&timezone=auto";

            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var at = DateTimeOffset.UtcNow;
            HttpResponseMessage res;

            try
            {
                res = await _http.SendAsync(req, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Open-Meteo failed | endpoint={Endpoint} | lat={Lat} | lon={Lon} | at={At}",
                    "forecast", latStr, lonStr, at);
                throw;
            }

            _logger.LogInformation(
                "Open-Meteo call | endpoint={Endpoint} | lat={Lat} | lon={Lon} | status={Status} | at={At}",
                "forecast", latStr, lonStr, (int)res.StatusCode, at);

            if (!res.IsSuccessStatusCode)
            {
                var body = await res.Content.ReadAsStringAsync(ct);
                _logger.LogError("Open-Meteo error body: {Body}", body);
                throw new HttpRequestException($"Open-Meteo retornou {(int)res.StatusCode}");
            }

            await using var stream = await res.Content.ReadAsStreamAsync(ct);
            var dto = await JsonSerializer.DeserializeAsync<WeatherForecastDto>(stream, JsonOptions, ct);

            if (dto?.daily is null ||
                dto.daily.time.Count == 0)
            {
                return null;
            }

            // Pega o primeiro dia retornado
            var dateStr = dto.daily.time[0];
            var min = dto.daily.temperature_2m_min?.ElementAtOrDefault(0);
            var max = dto.daily.temperature_2m_max?.ElementAtOrDefault(0);

            if (!DateOnly.TryParse(dateStr, out var date))
                date = DateOnly.FromDateTime(DateTime.UtcNow.Date);

            return new WeatherDailySummary
            {
                Date = date,
                Min = min,
                Max = max
            };
        })!;
    }
}
