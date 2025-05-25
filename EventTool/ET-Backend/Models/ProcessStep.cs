namespace ET_Backend.Models;
using ET_Backend.Models.Enums;

public class ProcessStep
{
    public int Id { get; set; }
    public string TypeName { get; set; }

    public ProcessStepType Type { get; set; }

    public ProcessStepTrigger Trigger { get; set; }

    public ProcessStepCondition Condition { get; set; }
    public int OffsetInHours { get; set; }
}