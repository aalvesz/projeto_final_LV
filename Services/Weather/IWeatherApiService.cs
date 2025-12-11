using projeto_final_LV.Models.Weather;

namespace projeto_final_LV.Services.Weather;

public interface IWeatherApiService
{
    Task<OpenMeteoForecastDto> GetDailyForecastAsync(double lat, double lon, CancellationToken ct = default);
}
