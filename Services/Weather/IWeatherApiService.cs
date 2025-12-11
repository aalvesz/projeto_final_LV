using projeto_final_LV.Models.Weather;

namespace projeto_final_LV.Services.Weather;

public interface IWeatherApiService
{
    Task<WeatherDailySummary?> GetDailySummaryAsync(
        double latitude,
        double longitude,
        CancellationToken ct = default);
}