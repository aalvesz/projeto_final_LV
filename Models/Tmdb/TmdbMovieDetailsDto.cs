using System.Text.Json.Serialization;

namespace projeto_final_LV.Models.Tmdb;

public sealed class TmdbMovieDetailsDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("original_title")]
    public string? OriginalTitle { get; set; }

    [JsonPropertyName("overview")]
    public string? Overview { get; set; }

    // TMDb manda a data como string (yyyy-MM-dd)
    [JsonPropertyName("release_date")]
    public string? ReleaseDate { get; set; }

    [JsonPropertyName("genres")]
    public List<TmdbGenreDto> Genres { get; set; } = [];

    [JsonPropertyName("poster_path")]
    public string? PosterPath { get; set; }

    [JsonPropertyName("original_language")]
    public string? OriginalLanguage { get; set; }

    [JsonPropertyName("runtime")]
    public int? Runtime { get; set; }

    [JsonPropertyName("vote_average")]
    public decimal? VoteAverage { get; set; }

    // ==== NOVO: créditos (elenco) ====
    [JsonPropertyName("credits")]
    public TmdbCreditsDto? Credits { get; set; }
}

public sealed class TmdbGenreDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

// bloco de créditos
public sealed class TmdbCreditsDto
{
    [JsonPropertyName("cast")]
    public List<TmdbCastDto> Cast { get; set; } = [];
}

public sealed class TmdbCastDto
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("character")]
    public string? Character { get; set; }

    [JsonPropertyName("order")]
    public int? Order { get; set; }
}
