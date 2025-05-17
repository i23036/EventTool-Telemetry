using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;

namespace ET_Backend.Repository;

/// <summary>
/// Stellt Erweiterungen und Hilfsmethoden für Datenbankoperationen bereit.
/// </summary>
public static class DbUtils
{
    /// <summary>
    /// Öffnet die Verbindung, falls sie noch geschlossen ist, und startet eine neue Transaktion.
    /// </summary>
    /// <param name="conn">Die Datenbankverbindung.</param>
    /// <returns>Eine offene Transaktion.</returns>
    public static IDbTransaction BeginSafeTransaction(this IDbConnection conn)
    {
        if (conn.State != ConnectionState.Open)
            conn.Open();
        return conn.BeginTransaction();
    }

    /// <summary>
    /// Prüft, ob die Verbindung eine SQLite-Verbindung ist.
    /// </summary>
    /// <param name="connection">Die zu prüfende Verbindung.</param>
    /// <returns>True, wenn es sich um SQLite handelt.</returns>
    public static bool IsSQLite(this IDbConnection connection) => connection is SqliteConnection;

    /// <summary>
    /// Prüft, ob die Verbindung eine SQL Server-Verbindung ist.
    /// </summary>
    /// <param name="connection">Die zu prüfende Verbindung.</param>
    /// <returns>True, wenn es sich um SQL Server handelt.</returns>
    public static bool IsSqlServer(this IDbConnection connection) => connection is SqlConnection;

    /// <summary>
    /// Gibt den Namen der erkannten Datenbankplattform zurück.
    /// </summary>
    /// <param name="connection">Die zu analysierende Verbindung.</param>
    /// <returns>Plattformname (z. B. "SQLite", "SQL Server" oder "Unbekannt").</returns>
    public static string GetPlatformName(this IDbConnection connection)
    {
        if (connection.IsSQLite()) return "SQLite";
        if (connection.IsSqlServer()) return "SQL Server";
        return "Unbekannt";
    }
}