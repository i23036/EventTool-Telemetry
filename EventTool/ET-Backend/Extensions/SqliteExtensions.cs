using Microsoft.Data.Sqlite;

namespace ET_Backend.Extensions;

public static class SqliteExtensions
{
    /// <summary>
    /// Aktiviert Foreign Keys für SQLite-Verbindungen.
    /// Muss nach dem Öffnen einmalig ausgeführt werden.
    /// </summary>
    public static SqliteConnection WithForeignKeys(this SqliteConnection conn)
    {
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "PRAGMA foreign_keys = ON;";
        cmd.ExecuteNonQuery();
        return conn;
    }
}