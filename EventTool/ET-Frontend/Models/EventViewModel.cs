public class EventViewModel
{
    public int    Id              { get; set; }
    public string Name            { get; set; } = "";
    public string Description     { get; set; } = "";

    public string StatusDisplay   { get; set; } = "";
    public bool   CanSubscribe    { get; set; }

    public int    Participants    { get; set; }
    public int    MaxParticipants { get; set; }
    public bool   IsOrganizer     { get; set; }
    public bool   IsSubscribed    { get; set; }
}