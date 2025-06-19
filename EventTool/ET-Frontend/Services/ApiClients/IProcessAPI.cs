using ET_Frontend.Models;

namespace ET_Frontend.Services.ApiClients
{
    public interface IProcessAPI
    {
        Task<ProcessViewModel> GetAsync(int eventId);
        Task<bool> UpdateAsync(int eventId, ProcessViewModel vm);
    }
}