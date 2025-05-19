using System.Net;
using System.Net.Mail;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ET_Backend.Services.Helper;

/// <summary>
/// SMTP-Implementierung von <see cref="IEMailService"/>.
/// </summary>
public sealed class EMailService : IEMailService
{
    private readonly EmailSettings _cfg;
    private readonly ILogger<EMailService> _log;

    public EMailService(IOptions<EmailSettings> opts,
        ILogger<EMailService>   log)
    {
        _cfg = opts.Value;
        _log = log;
    }

    public async Task SendAsync(string to, string subject, string htmlBody)
    {
        using var msg = new MailMessage
        {
            From           = new MailAddress(_cfg.FromAddress, _cfg.FromName, Encoding.UTF8),
            Subject        = subject,
            Body           = htmlBody,
            IsBodyHtml     = true,
            BodyEncoding    = Encoding.UTF8,
            SubjectEncoding = Encoding.UTF8
        };
        msg.To.Add(to);

        using var client = new SmtpClient(_cfg.SmtpServer, _cfg.Port)
        {
            EnableSsl = _cfg.UseSsl
        };
        if (!string.IsNullOrWhiteSpace(_cfg.UserName))
            client.Credentials =
                new NetworkCredential(_cfg.UserName, _cfg.Password);

        _log.LogInformation("Sende E-Mail an {Recipient}: {Subject}", to, subject);

        await Task.Run(() => client.Send(msg));   // blockiert nicht den ASP-Thread
    }
}