namespace ET_Backend.Models;

public class Process
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public List<ProcessStep> ProcessSteps { get; set; } = [];
}