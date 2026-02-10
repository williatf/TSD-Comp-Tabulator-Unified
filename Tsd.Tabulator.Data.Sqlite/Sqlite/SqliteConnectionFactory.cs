using Microsoft.Data.Sqlite;

namespace Tsd.Tabulator.Data.Sqlite;

public sealed class SqliteConnectionFactory(string dbPath) : ISqliteConnectionFactory
{
    private readonly string _cs = new SqliteConnectionStringBuilder
    {
        DataSource = dbPath,
        ForeignKeys = true,
        Mode = SqliteOpenMode.ReadWriteCreate,
        Cache = SqliteCacheMode.Shared
    }.ToString();

    public SqliteConnection OpenConnection()
    {
        var conn = new SqliteConnection(_cs);
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            PRAGMA foreign_keys = ON;
            PRAGMA journal_mode = WAL;
            PRAGMA synchronous = NORMAL;
        """;
        cmd.ExecuteNonQuery();

        return conn;
    }
}
