using ET.Shared.DTOs;
using ET.Shared.DTOs.Enums;

namespace ET_Frontend.Models.Event;

public class EventCreateViewModel
{
    public int Id { get; set; }
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

    public List<EventParticipantDto> Participants { get; set; } = new();
    public List<string> ContactPersons { get; set; } = new();
    public List<string> Managers { get; set; } = new();

    public EventStatus Status { get; set; } = EventStatus.Entwurf;
    public bool IsSubscribed { get; set; }
}