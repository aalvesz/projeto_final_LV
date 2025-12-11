using System.Text.Json.Serialization;

namespace projeto_final_LV.Models.Weather;

public sealed class OpenMeteoForecastDto
{
    [JsonPropertyName("daily")]
    public OpenMeteoDailyDto Daily { get; set; } = new();
}

public sealed class OpenMeteoDailyDto
{
    [JsonPropertyName("time")]
    public List<string> Time { get; set; } = [];

    [JsonPropertyName("temperature_2m_max")]
    public List<double> TemperatureMax { get; set; } = [];

    [JsonPropertyName("temperature_2m_min")]
    public List<double> TemperatureMin { get; set; } = [];
}
