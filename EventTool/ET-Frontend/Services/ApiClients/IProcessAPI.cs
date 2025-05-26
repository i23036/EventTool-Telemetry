using ET_Frontend.Models;

namespace ET_Frontend.Services.ApiClients
{
    public interface IProcessAPI
    {
        /// <summary>
        /// Aktualisiert den Prozess im Backend.
        /// </summary>
        /// <param name="model">Das geänderte ProcessModel mit Prozesschritten und Id.</param>
        /// <returns>True bei Erfolg, sonst false.</returns>
        Task<bool> UpdateProcessAsync(ProcessViewModel model);
    }
}
