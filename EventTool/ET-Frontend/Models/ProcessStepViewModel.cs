using ET.Shared.DTOs.Enums;

namespace ET_Frontend.Models;

public class ProcessStepViewModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public ProcessStepTrigger Trigger { get; set; }
    public ProcessStepAction Action { get; set; }
    public int Offset { get; set; }
    public int? TriggeredByStepId { get; set; }
    public string? Subject { get; set; }
    public string? Body    { get; set; }

    public ProcessStepViewModel() {}
}