namespace ET_Backend.Models;

/// <summary>
/// Repräsentiert ein Benutzerkonto mit zugehöriger Organisation und Rolle.
/// </summary>
public class Account
{
    /// <summary>
    /// Die E-Mail-Adresse des Benutzerkontos.
    /// </summary>
    public String EMail { set; get; }

    /// <summary>
    /// Die ID der Organisation, der das Konto zugeordnet ist.
    /// </summary>
    public int Organization { set; get; }

    /// <summary>
    /// Die Rolle des Benutzers im System.
    /// </summary>
    public Role Role { set; get; }
}