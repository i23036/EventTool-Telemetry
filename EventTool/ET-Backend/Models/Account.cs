using ET_Backend.Models.Enums;

namespace ET_Backend.Models;

/// <summary>
/// Repräsentiert ein Benutzerkonto mit zugehöriger Organisation und Rolle.
/// </summary>
public class Account
{
    public int Id { set; get; }

    public string EMail { set; get; }

    public int UserId { get; set; }

    public User User { set; get; }

    public Organization Organization { set; get; }

    public bool IsVerified { set; get; }

    public Role Role { set; get; }

    public List<Event> Events { set; get; } = new List<Event>();
}