namespace projeto_final_LV.Models.Options;

public sealed class TmdbOptions
{
    public string BaseUrl { get; set; } = "https://api.themoviedb.org/3/";
    public string? ApiKey { get; set; }
    public string? BearerToken { get; set; }
    public string Language { get; set; } = "pt-BR";
    public string ImageSize { get; set; } = "w185";
}
