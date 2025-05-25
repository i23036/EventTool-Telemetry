using ET_Frontend.Models;

namespace ET_Frontend.Services.ApiClients
{
    public interface IProcessAPI
    {
        /// <summary>
        /// Ruft den aktuellen Process vom Backend ab.
        /// </summary>
        /// <returns>Das ProcessModel mit den Prozesschritten und seiner Id.</returns>
        Task<ProcessViewModel?> GetCurrentProcessAsync();

        /// <summary>
        /// Aktualisiert den Prozess im Backend.
        /// </summary>
        /// <param name="model">Das geänderte ProcessModel mit Prozesschritten und Id.</param>
        /// <returns>True bei Erfolg, sonst false.</returns>
        Task<bool> UpdateProcessAsync(ProcessViewModel model);
    }
}
