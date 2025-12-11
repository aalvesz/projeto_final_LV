using projeto_final_LV.Models;
using projeto_final_LV.Models.Weather;

namespace projeto_final_LV.Models.ViewModels;

public sealed class MovieDetailsVm
{
    public Movie Movie { get; init; } = null!;
    public WeatherDailySummary? Weather { get; init; }
}
