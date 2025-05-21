using Bunit;
using Xunit;
using ET_Frontend.Pages.AccountManagement;
using MudBlazor.Services;
using Moq;
using Microsoft.Extensions.DependencyInjection;

namespace ET_UnitTests.Frontendtests
{
    public class LoginPageTests : TestContext
    {
        public LoginPageTests()
        {
            Services.AddMudServices();

            // Mock für LoginService registrieren, falls [Inject] ILoginService verwendet wird
            var mockLoginService = new Mock<ET_Frontend.Services.Authentication.ILoginService>();
            Services.AddSingleton(mockLoginService.Object);

            // Falls weitere Services benötigt werden, hier ebenfalls mocken und registrieren
        }

        [Fact]
        public void LoginPage_ShouldRenderAllElements()
        {
            JSInterop.SetupVoid("mudElementRef.addOnBlurEvent", _ => true);

            var cut = RenderComponent<Login>();

            Assert.Contains("Melden Sie sich an", cut.Markup);
            Assert.Contains("E-Mail", cut.Markup);
            Assert.Contains("Passwort", cut.Markup);
            Assert.Contains("Anmelden", cut.Markup);
            Assert.Contains("Registrieren", cut.Markup);
        }
    }
}
