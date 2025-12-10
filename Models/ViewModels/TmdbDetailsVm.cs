namespace projeto_final_LV.Models.ViewModels;

public sealed class TmdbDetailsVm
{
    public int TmdbId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? TituloOriginal { get; set; }
    public string? Sinopse { get; set; }
    public string? DataLancamento { get; set; }
    public string? Generos { get; set; }
    public int? Duracao { get; set; }
    public decimal? NotaMedia { get; set; }
    public string? Lingua { get; set; }
    public string? PosterUrl { get; set; }
    public List<string> PosterUrlsExtras { get; set; } = [];
}
