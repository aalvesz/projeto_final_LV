namespace projeto_final_LV.Models;

public static class CityLocationHelper
{
    // Use exatamente os mesmos valores do <option value="..."> do layout
    private static readonly Dictionary<string, (double Lat, double Lon)> _cities =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["Ariquemes"] = (-9.9070, -63.0326),
            ["Ji-Parana"] = (-10.8777, -61.9326),
            ["Ouro Preto d'Oeste"] = (-10.7167, -62.2561),
            ["Presidente Médici"] = (-11.1696, -61.8986),
            ["Vilhena"] = (-12.7406, -60.1458),
        };

    public static bool TryGetCoordinates(string? city, out double lat, out double lon)
    {
        lat = 0;
        lon = 0;

        if (string.IsNullOrWhiteSpace(city))
            return false;

        if (_cities.TryGetValue(city, out var coord))
        {
            lat = coord.Lat;
            lon = coord.Lon;
            return true;
        }

        return false;
    }
}
