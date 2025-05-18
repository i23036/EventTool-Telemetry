using Bunit;
using Xunit;
using ET_Frontend.Pages.AccountManagement;
using MudBlazor.Services;

namespace ET_UnitTests.Frontendtests
{
    public class LoginPageTests : TestContext
    {
        public LoginPageTests()
        {
            Services.AddMudServices();
        }

        [Fact]
        public void LoginPage_ShouldRenderAllElements()
        {
            // JSInterop für MudBlazor-Events mocken
            JSInterop.SetupVoid("mudElementRef.addOnBlurEvent", _ => true);

            // Act
            var cut = RenderComponent<Login>();

            // Assert: Überschrift vorhanden
            cut.Markup.Contains("Melden Sie sich an");

            // Assert: E-Mail- und Passwortfeld vorhanden
            Assert.Contains("E-Mail", cut.Markup);
            Assert.Contains("Passwort", cut.Markup);

            // Assert: Anmelde-Button vorhanden
            Assert.Contains("Anmelden", cut.Markup);

            // Assert: Link zum Registrieren vorhanden
            Assert.Contains("Registrieren", cut.Markup);
        }
    }
}
