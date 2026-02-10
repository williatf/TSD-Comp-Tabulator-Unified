using Microsoft.Data.Sqlite;

namespace Tsd.Tabulator.Data.Sqlite;

public interface ISqliteConnectionFactory
{
    SqliteConnection OpenConnection();
}
