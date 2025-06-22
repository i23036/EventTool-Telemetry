namespace ET_Frontend.Models;

public class ProcessViewModel
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public List<ProcessStepViewModel> ProcessSteps { get; set; }

    public ProcessViewModel() { }

    public ProcessViewModel(int id, int eventId, List<ProcessStepViewModel> processSteps)
    {
        Id = id;
        EventId = eventId;
        ProcessSteps = processSteps;
    }
}