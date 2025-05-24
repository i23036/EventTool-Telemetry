namespace ET_Frontend.Models;

public class ProcessModel
{
    public int ID{ get; set; }
    public List<ProcessStepModel> ProcessSteps { get; set; }

    public ProcessModel() { }

    public ProcessModel(int id, List<ProcessStepModel> ProcessSteps)
    {
        this.ID = id;
        this.ProcessSteps = ProcessSteps;
    }
}
