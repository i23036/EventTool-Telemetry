namespace ET_Backend.Services.Helper;

/// <summary>
/// Konfiguration für den <see cref="EMailService"/>.
/// Werte kommen aus <c>appsettings*.json</c> oder Umgebungsvariablen.
/// </summary>
public sealed class EmailSettings
{
    public string SmtpServer  { get; init; } = string.Empty;
    public int    Port        { get; init; } = 25;
    public bool   UseSsl      { get; init; }
    public string? UserName   { get; init; }
    public string? Password   { get; init; }
    public string FromAddress { get; init; } = "noreply@event-tool.local";
    public string FromName    { get; init; } = "Event-Tool";
}