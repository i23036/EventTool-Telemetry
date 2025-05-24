namespace ET_Frontend.Models.Event;

public class EventViewModel
{
    public int EventId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Participants { get; set; }
    public int MaxParticipants { get; set; }
    public bool IsOrganizer { get; set; }
    public bool IsSubscribed { get; set; }
}