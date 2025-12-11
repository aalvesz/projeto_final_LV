using System.Text.Json.Serialization;

namespace projeto_final_LV.Models.Tmdb;

public sealed class TmdbConfigurationDto
{
    [JsonPropertyName("images")]
    public TmdbImagesConfigDto Images { get; set; } = new();
}

public sealed class TmdbImagesConfigDto
{
    [JsonPropertyName("secure_base_url")]
    public string SecureBaseUrl { get; set; } = string.Empty;

    [JsonPropertyName("poster_sizes")]
    public List<string> PosterSizes { get; set; } = [];
}
