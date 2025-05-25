namespace ET_Frontend.Models;
using Models.Enums;

public class ProcessStepViewModel
{
    public int Id { get; set; }
    public string TypeName { get; set; }

    public ProcessStepType Type { get; set; }

    public ProcessStepTrigger Trigger { get; set; }

    public ProcessStepCondition Condition { get; set; }
    public int OffsetInHours { get; set; }

    public ProcessStepViewModel() {}

    public ProcessStepViewModel(int Id, string TypeName, ProcessStepType Type, ProcessStepTrigger Trigger, ProcessStepCondition Condition, int OffsetInHours)
    {
        this.Id = Id;
        this.TypeName = TypeName;
        this.Type = Type;
        this.Trigger = Trigger;
        this.Condition = Condition;
        this.OffsetInHours = OffsetInHours;
    }
}