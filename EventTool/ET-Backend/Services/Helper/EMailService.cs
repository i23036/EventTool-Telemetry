using System.Net;
using System.Net.Mail;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ET_Backend.Services.Helper;

/// <summary>
/// SMTP-basierte Implementierung von <see cref="IEMailService"/>.
/// </summary>
public sealed class EMailService : IEMailService
{
    private readonly EmailSettings          _settings;
    private readonly ILogger<EMailService> _logger;

    public EMailService(IOptions<EmailSettings> options,
        ILogger<EMailService>   logger)
    {
        _settings = options.Value;
        _logger   = logger;
    }

    /// <inheritdoc />
    public async Task SendAsync(string to, string subject, string htmlBody)
    {
        using var msg = new MailMessage
        {
            From           = new MailAddress(_settings.FromAddress,
                _settings.FromName,
                Encoding.UTF8),
            Subject        = subject,
            Body           = htmlBody,
            IsBodyHtml     = true,
            BodyEncoding    = Encoding.UTF8,
            SubjectEncoding = Encoding.UTF8
        };
        msg.To.Add(to);

        using var client = new SmtpClient(_settings.SmtpServer, _settings.Port)
        {
            EnableSsl = _settings.UseSsl
        };
        if (!string.IsNullOrWhiteSpace(_settings.UserName))
            client.Credentials =
                new NetworkCredential(_settings.UserName, _settings.Password);

        _logger.LogInformation("Sende E-Mail an {Recipient}: {Subject}", to, subject);

        // SmtpClient.Send ist synchron – wir wrappen ihn, damit der ASP-Thread nicht blockiert.
        await Task.Run(() => client.Send(msg));
    }
}