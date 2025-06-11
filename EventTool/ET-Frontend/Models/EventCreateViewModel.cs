using ET.Shared.DTOs.Enums;

namespace ET_Frontend.Models.Event;

public class EventCreateViewModel
{
    public string Name { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public int MinUsers { get; set; }
    public int MaxUsers { get; set; }

    public DateTime? RegistrationStart { get; set; }
    public DateTime? RegistrationDeadline { get; set; }

    // kann jeder aus der Orga sein
    public List<string> ContactPersons { get; set; } = new();
    
    // Managers (Verwalter), nur Owner & Organisatoren
    public List<string> Managers { get; set; } = new();

    // Event-Status (Dropdown, Enum)
    public EventStatus Status { get; set; } = EventStatus.Entwurf;
}