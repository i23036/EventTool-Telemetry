namespace ET_Backend.Models;

public class Event
{
    public int Id { set; get; }

    public string Name { set; get; }

    public string Description { set; get; }

    public List<Account> Participants { set; get; } = new List<Account>();

    public List<Account> Organizers { set; get; } = new List<Account>();

    public List<Account> ContactPersons { set; get; } = new List<Account>();

    public Organization Organization { get; set; }

    public Process Process { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public string Location { get; set; }

    public int MinParticipants { get; set; }

    public int MaxParticipants { get; set; }

    public DateOnly RegistrationStart { get; set; }

    public DateOnly RegistrationEnd { get; set; }

    public bool IsBlueprint { get; set; }

}