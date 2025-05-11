namespace ET_Backend.Models;
/// <summary>
/// Repräsentiert eine Organisation mit Lastname, Beschreibung, Domain und zugehörigen Mitgliedern.
/// </summary>
public class Organization
{
    public int Id { get; set; }

    public String Name { set; get; }

    public String Description { set; get; }

    public String Domain { set; get; }

    public List<Event> Events { set; get; } = new List<Event>();

    public List<Account> Accounts { set; get; } = new List<Account>();

    public List<Process> Processes { set; get; } = new List<Process>();

    public List<ProcessStep> ProcessSteps { set; get; } = new List<ProcessStep>();
}