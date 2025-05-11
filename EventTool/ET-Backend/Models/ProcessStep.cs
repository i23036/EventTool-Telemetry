namespace ET_Backend.Models;

public class ProcessStep
{
    public int Id { get; set; }

    public string Name { get; set; }

    public int OrganizationId { get; set; }

    public int TriggerId { get; set; }
}