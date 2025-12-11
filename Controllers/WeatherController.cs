using Microsoft.AspNetCore.Mvc;
using projeto_final_LV.Models;
using projeto_final_LV.Services.Weather;

namespace projeto_final_LV.Controllers;

[Route("[controller]")]
public sealed class WeatherController : Controller
{
    private readonly IWeatherApiService _weather;

    public WeatherController(IWeatherApiService weather)
    {
        _weather = weather;
    }

    [HttpGet("ByCity")]
    public async Task<IActionResult> ByCity(string city, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(city))
            return BadRequest(new { ok = false, error = "Cidade obrigatória." });

        if (!CityLocationHelper.TryGetCoordinates(city, out var lat, out var lon))
            return BadRequest(new { ok = false, error = "Cidade desconhecida." });

        var summary = await _weather.GetDailySummaryAsync(lat, lon, ct);
        if (summary is null)
            return Ok(new { ok = false, city, error = "Sem dados de previsão." });

        return Ok(new
        {
            ok = true,
            city,
            date = summary.Date.ToString("yyyy-MM-dd"),
            min = summary.Min,
            max = summary.Max
        });
    }
}
