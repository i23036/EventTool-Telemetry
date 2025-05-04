namespace ET_Backend.Models;
/// <summary>
/// Repräsentiert eine Organisation mit Name, Beschreibung, Domain und zugehörigen Mitgliedern.
/// </summary>
public class Organization
{
    /// <summary>
    /// Der Name der Organisation.
    /// </summary>
    public String Name { set; get; }

    /// <summary>
    /// Eine optionale Beschreibung der Organisation.
    /// </summary>
    public String Description { set; get; }

    /// <summary>
    /// Die Domain, unter der die Organisation operiert (z. B. example.com).
    /// </summary>
    public String Domain { set; get; }

    /// <summary>
    /// Die Liste der Mitglieder (Benutzerkonten), die zur Organisation gehören.
    /// </summary>
    public List<Account> Members { set; get; } = new List<Account>();
}