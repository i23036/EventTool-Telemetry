namespace ET_Frontend.Models;

public class ProcessViewModel
{
    public int Id{ get; set; }
    public List<ProcessStepViewModel> ProcessSteps { get; set; }

    public ProcessViewModel() { }

    public ProcessViewModel(int id, List<ProcessStepViewModel> ProcessSteps)
    {
        this.Id = id;
        this.ProcessSteps = ProcessSteps;
    }
}
