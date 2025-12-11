using Microsoft.Data.Sqlite;

namespace projeto_final_LV.Data;

public static class DatabaseInitializer
{
    public static void EnsureCreated(string connectionString)
    {
        EnsureDbFolder(connectionString);

        using var con = new SqliteConnection(connectionString);
        con.Open();

        using var cmd = con.CreateCommand();
        cmd.CommandText =
        """
        CREATE TABLE IF NOT EXISTS Movies (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            TmdbId INTEGER NOT NULL,
            Titulo TEXT NOT NULL,
            TituloOriginal TEXT NULL,
            Sinopse TEXT NULL,
            DataLancamento TEXT NULL,
            Genero TEXT NULL,
            PosterPath TEXT NULL,
            Lingua TEXT NULL,
            Duracao INTEGER NULL,
            NotaMedia REAL NULL,
            ElencoPrincipal TEXT NULL,
            CidadeReferencia TEXT NULL,
            Latitude REAL NULL,
            Longitude REAL NULL,
            DataCriacao TEXT NOT NULL,
            DataAtualizacao TEXT NOT NULL
        );

        CREATE UNIQUE INDEX IF NOT EXISTS UX_Filmes_TmdbId ON Movies (TmdbId);
        """;
        cmd.ExecuteNonQuery();
    }

    private static void EnsureDbFolder(string connectionString)
    {
        const string prefix = "Data Source=";
        if (!connectionString.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            return;

        var path = connectionString[prefix.Length..].Trim();
        var dir = Path.GetDirectoryName(path);

        if (!string.IsNullOrWhiteSpace(dir))
            Directory.CreateDirectory(dir);
    }
}
