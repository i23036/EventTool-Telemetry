using System.Data;
using Dapper;
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
    /// Liefert den vollqualifizierten Tabellennamen – unter SQL Server wird automatisch "dbo." vorangestellt.
    /// </summary>
    public static string Tbl(this IDbConnection db, string baseName)
        => db.IsSQLite() ? baseName : $"dbo.{baseName}";

    /// <summary>
    /// Führt ein INSERT aus und gibt die generierte ID zurück (SQLite/SQL Server kompatibel).
    /// </summary>
    public static async Task<int> InsertAndGetIdAsync(this IDbConnection db, string sql, object param, IDbTransaction? tx = null)
    {
        if (db.IsSQLite())
        {
            await db.ExecuteAsync(sql, param, tx);
            var id = await db.ExecuteScalarAsync<long>("SELECT last_insert_rowid();", transaction: tx);
            return (int)id;
        }
        else
        {
            var fullSql = $"{sql}; SELECT CAST(SCOPE_IDENTITY() AS int);";
            return await db.ExecuteScalarAsync<int>(fullSql, param, tx);
        }
    }


    /// <summary>
    /// Gibt den Split-Typ für Rollen zurück (long in SQLite, int in SQL Server).
    /// </summary>
    public static Type RoleSplitType(this IDbConnection db)
        => db.IsSQLite() ? typeof(long) : typeof(int);
    
    /// <summary>
    /// Liest eine Bilddatei vom angegebenen relativen Pfad (bezogen auf das aktuelle Arbeitsverzeichnis)
    /// und konvertiert deren Inhalt in einen Base64-codierten String.
    /// Diese Methode wird z. B. beim Initial-Seeding von Bilddaten (z. B. Logos) verwendet.
    /// </summary>
    /// <param name="relativePath">
    /// Relativer Pfad zur Bilddatei, z. B. "Resources/Seed/BitWorksSimpel-Gro.png".
    /// Der Pfad wird relativ zu <c>Directory.GetCurrentDirectory()</c> aufgelöst.
    /// </param>
    /// <returns>
    /// Ein Base64-codierter String des Bildes – <b>ohne</b> "data:image/...;base64,"-Präfix.
    /// Gibt <c>null</c> zurück, wenn die Datei nicht gefunden oder nicht gelesen werden konnte.
    /// </returns>
    public static string GetBase64FromImage(string relativePath)
    {
        try
        {
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), relativePath);
            if (!File.Exists(fullPath))
                return null;

            var bytes = File.ReadAllBytes(fullPath);
            return Convert.ToBase64String(bytes);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WARN] Fehler beim Lesen der Bilddatei: {ex.Message}");
            return null;
        }
    }
}