using ET.Shared.DTOs.Enums;

namespace ET_Backend.Models;

public class Event
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;

    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }

    public int MinParticipants { get; set; }
    public int MaxParticipants { get; set; }

    public DateOnly RegistrationStart { get; set; }
    public DateOnly RegistrationEnd { get; set; }

    public List<Account> Organizers { get; set; } = [];
    public List<Account> ContactPersons { get; set; } = [];
    public List<Account> Participants { get; set; } = [];

    public Organization Organization { get; set; } = null!;
    public Process? Process { get; set; }

    public EventStatus Status { get; set; } = EventStatus.Entwurf;

    public bool IsBlueprint { get; set; }
}