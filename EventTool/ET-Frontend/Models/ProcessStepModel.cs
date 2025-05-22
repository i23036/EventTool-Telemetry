namespace ET_Frontend.Models;
using OneOf;

    public class ProcessStepModel
{
    public string TypeName { get; set; }

    public ProcessStepType Type { get; set; }

    public ProcessStepTrigger Trigger { get; set; }

    public ProcessStepCondition Condition { get; set; }
    public int OffsetInHours { get; set; }

    public ProcessStepModel() {}

    public ProcessStepModel(string TypeName, ProcessStepType Type, ProcessStepTrigger Trigger, ProcessStepCondition Condition, int OffsetInHours)
    {
        this.TypeName = TypeName;
        this.Type = Type;
        this.Trigger = Trigger;
        this.Condition = Condition;
        this.OffsetInHours = OffsetInHours;
    }
}