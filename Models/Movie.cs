using System;
using System.ComponentModel.DataAnnotations;

namespace projeto_final_LV.Models;

public sealed class Movie
{
    public int Id { get; set; }

    [Required]
    public int TmdbId { get; set; }

    [Required, StringLength(300)]
    public string Titulo { get; set; } = string.Empty;

    [StringLength(300)]
    public string? TituloOriginal { get; set; }

    public string? Sinopse { get; set; }

    public DateOnly? DataLancamento { get; set; }

    [StringLength(200)]
    public string? Genero { get; set; }

    [StringLength(500)]
    public string? PosterPath { get; set; }

    [StringLength(10)]
    public string? Lingua { get; set; }

    [Range(0, 2000)]
    public int? Duracao { get; set; }

    [Range(0, 10)]
    public decimal? NotaMedia { get; set; }

    [StringLength(1000)]
    public string? ElencoPrincipal { get; set; }

    [StringLength(200)]
    public string? CidadeReferencia { get; set; }

    [Range(-90, 90)]
    public double? Latitude { get; set; }

    [Range(-180, 180)]
    public double? Longitude { get; set; }

    public DateTimeOffset DataCriacao { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset DataAtualizacao { get; set; } = DateTimeOffset.UtcNow;

    public void Touch() => DataAtualizacao = DateTimeOffset.UtcNow;
}
