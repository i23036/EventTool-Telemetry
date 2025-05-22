using System.Text.Json.Serialization;

namespace ET.Shared.DTOs;

/// <summary>
/// Repräsentiert ein Organisationsmitglied mit Rolle, Nachname und E-Mail.
/// </summary>
public class OrganizationMemberDto
{
    public string Email { get; set; }
    public string Lastname { get; set; }
    public int Role { get; set; }

    // Parameterloser Konstruktor für System.Text.Json
    public OrganizationMemberDto() { }

    // Optional: Hauptkonstruktor
    [JsonConstructor]
    public OrganizationMemberDto(string email, string lastname, int role)
    {
        Email = email;
        Lastname = lastname;
        Role = role;
    }
}