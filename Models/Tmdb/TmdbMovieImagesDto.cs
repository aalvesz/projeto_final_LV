using System.Text.Json.Serialization;

namespace projeto_final_LV.Models.Tmdb;

public sealed class TmdbMovieImagesDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("posters")]
    public List<TmdbImageItemDto> Posters { get; set; } = [];
}

public sealed class TmdbImageItemDto
{
    [JsonPropertyName("file_path")]
    public string FilePath { get; set; } = string.Empty;
}
