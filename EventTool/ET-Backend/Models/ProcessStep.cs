using ET.Shared.DTOs.Enums;

namespace ET_Backend.Models;

public class ProcessStep
{
    public int Id { get; set; }
    public string Name { get; set; }
    public ProcessStepTrigger Trigger { get; set; }
    public ProcessStepAction Action { get; set; }
    public int? Offset { get; set; }
    public int? TriggeredByStepId { get; set; }
}