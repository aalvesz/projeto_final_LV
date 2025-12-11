namespace projeto_final_LV.Models.ViewModels;

public sealed class WeatherBlockVm
{
    public bool HasCoordinates { get; set; }
    public string? Date { get; set; }
    public double? Min { get; set; }
    public double? Max { get; set; }
    public string? Message { get; set; }
}
