namespace ET_Backend.Services.Helper;

/// <summary>
/// Abstrakte Schnittstelle zum Versenden von E-Mails.<br/>
/// Wird überall dort injiziert, wo Mails verschickt werden (z. B. Registrierung,
/// Erinnerungen). So kann der Dienst im Testfall gemockt werden.
/// </summary>
public interface IEMailService
{
    /// <summary>
    /// Versendet eine HTML-E-Mail.
    /// </summary>
    /// <param name="to">Empfängeradresse.</param>
    /// <param name="subject">Betreff.</param>
    /// <param name="htmlBody">HTML-Body (UTF-8).</param>
    Task SendAsync(string to, string subject, string htmlBody);
}