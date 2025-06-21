using ET.Shared.DTOs.Enums;

namespace ET_Frontend.Models;

public class ProcessStepViewModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public ProcessStepTrigger Trigger { get; set; }
    public ProcessStepAction Action { get; set; }
    public int Offset { get; set; }
    public int TriggeredByStepId { get; set; }

    public ProcessStepViewModel() {}

    public ProcessStepViewModel(int id, string name, ProcessStepTrigger trigger, ProcessStepAction action, int offset, int triggeredByStepId)
    {
        this.Id = id;
        this.Name = name;
        this.Action = action;
        this.Trigger = trigger;
        this.Offset = offset;
        this.TriggeredByStepId = triggeredByStepId;
    }
}