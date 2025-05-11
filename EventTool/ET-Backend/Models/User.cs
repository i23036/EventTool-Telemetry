namespace ET_Backend.Models;
/// <summary>
/// Repräsentiert einen Benutzer mit Namen und Passwort.
/// </summary>
public class User
{
    public int Id { get; set; }
    public String Lastname { set; get; }

    public String Firstname { set; get; }

    public String Password { set; get; }

    public List<Account> Accounts { get; set; } = new List<Account>();
}