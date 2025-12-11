using Microsoft.Data.Sqlite;
using projeto_final_LV.Models;

namespace projeto_final_LV.Repositories;

public sealed class MovieRepository : IMovieRepository
{
    private readonly string _cs;

    public MovieRepository(IConfiguration cfg)
    {
        _cs = cfg.GetConnectionString("LocalDb")
              ?? throw new InvalidOperationException("ConnectionStrings:LocalDb não configurado.");
    }

    public async Task<int> CreateAsync(Movie f, CancellationToken ct = default)
    {
        await using var con = new SqliteConnection(_cs);
        await con.OpenAsync(ct);

        await using var cmd = con.CreateCommand();
        cmd.CommandText =
        """
        INSERT INTO Movies (
            TmdbId, Titulo, TituloOriginal, Sinopse, DataLancamento, Genero, PosterPath, Lingua,
            Duracao, NotaMedia, ElencoPrincipal, CidadeReferencia, Latitude, Longitude,
            DataCriacao, DataAtualizacao
        ) VALUES (
            @TmdbId, @Titulo, @TituloOriginal, @Sinopse, @DataLancamento, @Genero, @PosterPath, @Lingua,
            @Duracao, @NotaMedia, @ElencoPrincipal, @CidadeReferencia, @Latitude, @Longitude,
            @DataCriacao, @DataAtualizacao
        );

        SELECT last_insert_rowid();
        """;

        Bind(cmd, f);

        var id = (long)(await cmd.ExecuteScalarAsync(ct) ?? 0L);
        return (int)id;
    }

    public async Task<Movie?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        await using var con = new SqliteConnection(_cs);
        await con.OpenAsync(ct);

        await using var cmd = con.CreateCommand();
        cmd.CommandText = "SELECT * FROM Movies WHERE Id = @Id LIMIT 1;";
        cmd.Parameters.AddWithValue("@Id", id);

        await using var r = await cmd.ExecuteReaderAsync(ct);
        return await r.ReadAsync(ct) ? ReadMovie(r) : null;
    }

    public async Task<Movie?> GetByTmdbIdAsync(int tmdbId, CancellationToken ct = default)
    {
        await using var con = new SqliteConnection(_cs);
        await con.OpenAsync(ct);

        await using var cmd = con.CreateCommand();
        cmd.CommandText = "SELECT * FROM Movies WHERE TmdbId = @TmdbId LIMIT 1;";
        cmd.Parameters.AddWithValue("@TmdbId", tmdbId);

        await using var r = await cmd.ExecuteReaderAsync(ct);
        return await r.ReadAsync(ct) ? ReadMovie(r) : null;
    }

    public async Task<IReadOnlyList<Movie>> ListAsync(CancellationToken ct = default)
    {
        var list = new List<Movie>();

        await using var con = new SqliteConnection(_cs);
        await con.OpenAsync(ct);

        await using var cmd = con.CreateCommand();
        cmd.CommandText = "SELECT * FROM Movies ORDER BY DataCriacao DESC;";

        await using var r = await cmd.ExecuteReaderAsync(ct);
        while (await r.ReadAsync(ct))
            list.Add(ReadMovie(r));

        return list;
    }

    public async Task UpdateAsync(Movie f, CancellationToken ct = default)
    {
        await using var con = new SqliteConnection(_cs);
        await con.OpenAsync(ct);

        await using var cmd = con.CreateCommand();
        cmd.CommandText =
        """
        UPDATE Movies SET
            TmdbId=@TmdbId,
            Titulo=@Titulo,
            TituloOriginal=@TituloOriginal,
            Sinopse=@Sinopse,
            DataLancamento=@DataLancamento,
            Genero=@Genero,
            PosterPath=@PosterPath,
            Lingua=@Lingua,
            Duracao=@Duracao,
            NotaMedia=@NotaMedia,
            ElencoPrincipal=@ElencoPrincipal,
            CidadeReferencia=@CidadeReferencia,
            Latitude=@Latitude,
            Longitude=@Longitude,
            DataAtualizacao=@DataAtualizacao
        WHERE Id=@Id;
        """;

        cmd.Parameters.AddWithValue("@Id", f.Id);
        Bind(cmd, f);

        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        await using var con = new SqliteConnection(_cs);
        await con.OpenAsync(ct);

        await using var cmd = con.CreateCommand();
        cmd.CommandText = "DELETE FROM Movies WHERE Id=@Id;";
        cmd.Parameters.AddWithValue("@Id", id);

        await cmd.ExecuteNonQueryAsync(ct);
    }

    private static void Bind(SqliteCommand cmd, Movie f)
    {
        cmd.Parameters.AddWithValue("@TmdbId", f.TmdbId);
        cmd.Parameters.AddWithValue("@Titulo", f.Titulo);
        cmd.Parameters.AddWithValue("@TituloOriginal", (object?)f.TituloOriginal ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Sinopse", (object?)f.Sinopse ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@DataLancamento", f.DataLancamento?.ToString("yyyy-MM-dd") ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@Genero", (object?)f.Genero ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@PosterPath", (object?)f.PosterPath ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Lingua", (object?)f.Lingua ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Duracao", (object?)f.Duracao ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@NotaMedia", (object?)f.NotaMedia ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ElencoPrincipal", (object?)f.ElencoPrincipal ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@CidadeReferencia", (object?)f.CidadeReferencia ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Latitude", (object?)f.Latitude ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Longitude", (object?)f.Longitude ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@DataCriacao", f.DataCriacao.ToString("O"));
        cmd.Parameters.AddWithValue("@DataAtualizacao", f.DataAtualizacao.ToString("O"));
    }

    private static Movie ReadMovie(SqliteDataReader r)
    {
        DateOnly? ParseDateOnly(object v)
        {
            if (v is DBNull) return null;
            return DateOnly.TryParse(v.ToString(), out var d) ? d : null;
        }

        DateTimeOffset ParseDto(object v)
        {
            if (v is DBNull) return DateTimeOffset.UtcNow;
            return DateTimeOffset.TryParse(v.ToString(), out var d) ? d : DateTimeOffset.UtcNow;
        }

        return new Movie
        {
            Id = Convert.ToInt32(r["Id"]),
            TmdbId = Convert.ToInt32(r["TmdbId"]),
            Titulo = Convert.ToString(r["Titulo"]) ?? string.Empty,
            TituloOriginal = r["TituloOriginal"] is DBNull ? null : Convert.ToString(r["TituloOriginal"]),
            Sinopse = r["Sinopse"] is DBNull ? null : Convert.ToString(r["Sinopse"]),
            DataLancamento = ParseDateOnly(r["DataLancamento"]),
            Genero = r["Genero"] is DBNull ? null : Convert.ToString(r["Genero"]),
            PosterPath = r["PosterPath"] is DBNull ? null : Convert.ToString(r["PosterPath"]),
            Lingua = r["Lingua"] is DBNull ? null : Convert.ToString(r["Lingua"]),
            Duracao = r["Duracao"] is DBNull ? null : Convert.ToInt32(r["Duracao"]),
            NotaMedia = r["NotaMedia"] is DBNull ? null : Convert.ToDecimal(r["NotaMedia"]),
            ElencoPrincipal = r["ElencoPrincipal"] is DBNull ? null : Convert.ToString(r["ElencoPrincipal"]),
            CidadeReferencia = r["CidadeReferencia"] is DBNull ? null : Convert.ToString(r["CidadeReferencia"]),
            Latitude = r["Latitude"] is DBNull ? null : Convert.ToDouble(r["Latitude"]),
            Longitude = r["Longitude"] is DBNull ? null : Convert.ToDouble(r["Longitude"]),
            DataCriacao = ParseDto(r["DataCriacao"]),
            DataAtualizacao = ParseDto(r["DataAtualizacao"])
        };
    }
}
