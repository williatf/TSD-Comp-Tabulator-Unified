using System.Data.Common;
using Microsoft.Data.Sqlite;
using Tsd.Tabulator.Data.Sqlite;

namespace Tsd.Tabulator.Tests;

/// <summary>
/// Creates in-memory SQLite connections for testing.
/// </summary>
public sealed class InMemorySqliteConnectionFactory : ISqliteConnectionFactory
{
    private readonly string _connectionString = "Data Source=:memory:";

    public SqliteConnection OpenConnection()
    {
        var conn = new SqliteConnection(_connectionString);
        conn.Open();
        return conn;
    }
}