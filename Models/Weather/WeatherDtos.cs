using System.Text.Json.Serialization;

namespace projeto_final_LV.Models.Weather;

// DTO bruto da Open-Meteo
public sealed class WeatherForecastDto
{
    public double latitude { get; set; }
    public double longitude { get; set; }
    public string timezone { get; set; } = string.Empty;
    public WeatherDailyDto? daily { get; set; }
}

public sealed class WeatherDailyDto
{
    public List<string> time { get; set; } = new();
    public List<double>? temperature_2m_max { get; set; }
    public List<double>? temperature_2m_min { get; set; }
}

// Objeto simplificado para a aplicação
public sealed class WeatherDailySummary
{
    public DateOnly Date { get; set; }
    public double? Min { get; set; }
    public double? Max { get; set; }
}
