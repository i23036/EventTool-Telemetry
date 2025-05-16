namespace ET_Backend.Models;
/// <summary>
/// Repräsentiert einen Benutzer mit Namen und Passwort.
/// </summary>
public class User
{
    public int Id { get; set; }
    public string Lastname { set; get; }

    public string Firstname { set; get; }

    public string Password { set; get; }

    public List<Account> Accounts { get; set; } = new List<Account>();
}