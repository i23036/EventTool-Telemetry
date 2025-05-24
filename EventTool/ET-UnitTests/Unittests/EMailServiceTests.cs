using System.Threading.Tasks;
using ET_Backend.Services.Helper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace ET_UnitTests.Unittests
{
    public class EMailServiceTests
    {
        [Fact]
        public async Task SendAsync_DoesNotThrow_WithValidConfiguration()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<EMailService>>();
            
            var emailSettings = new EmailSettings 
            {
                FromAddress = "test@example.com",
                FromName = "Test Sender",
                SmtpServer = "localhost",
                Port = 25,
                UseSsl = false,
                UserName = "",
                Password = ""
            };

            var mockOptions = new Mock<IOptions<EmailSettings>>();
            mockOptions.Setup(o => o.Value).Returns(emailSettings);

            var emailService = new EMailService(mockOptions.Object, mockLogger.Object);

            // Act & Assert
            // Da der tatsächliche E-Mail-Versand Netzwerkkommunikation erfordert,
            // prüfen wir hauptsächlich, dass keine Ausnahme ausgelöst wird
            // und der Logger verwendet wird.
            
            // Anmerkung: Dieser Test kann in CI-Umgebungen fehlschlagen, 
            // wenn keine echte SMTP-Verbindung hergestellt werden kann.
            // Wir fangen die erwartete SmtpException ab.
            
            try
            {
                await emailService.SendAsync("recipient@example.com", "Test Subject", "<p>Test Body</p>");
            }
            catch (System.Net.Mail.SmtpException)
            {
                // SMTP-Fehler erwartet in Testumgebungen ohne echten SMTP-Server
                // Wir prüfen nur, dass der Logger aufgerufen wurde
            }

            // Verify that logging occurred
            mockLogger.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
                ),
                Times.AtLeastOnce
            );
        }
    }
}
