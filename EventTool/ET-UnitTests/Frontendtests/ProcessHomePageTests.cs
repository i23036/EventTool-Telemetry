using Bunit;
using Xunit;
using ET_Frontend.Pages.ProcessSuite;
using MudBlazor.Services;

namespace ET_UnitTests.Frontendtests
{
    public class ProcessHomePageTests : TestContext
    {
        public ProcessHomePageTests()
        {
            Services.AddMudServices();
            // Falls die Komponente weitere Services benötigt, hier mocken und registrieren
        }

        [Fact]
        public void ProcessHomePage_ShouldRenderAllElements()
        {
            // Beispiel: Passe die erwarteten Texte an deine Komponente an!
            var cut = RenderComponent<ProcessHome>();

            // ASSERTIONS MÜSSEN ANGEPASST WERDEN AN TATSÄCHLICHE KEYWORDS IN PROCESSHOME!!!
            Assert.Contains("Prozess", cut.Markup, System.StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Start", cut.Markup, System.StringComparison.OrdinalIgnoreCase);
        }
    }
}
