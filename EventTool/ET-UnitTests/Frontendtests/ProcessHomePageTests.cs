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
            JSInterop.SetupVoid("mudElementRef.addOnBlurEvent", _ => true);
        }

        [Fact]
        public void ProcessHomePage_ShouldRenderAllElements()
        {
            // Act
            var cut = RenderComponent<ProcessHome>();

            // Assert: Überschrift oder zentrale Elemente prüfen
            Assert.Contains("Prozess", cut.Markup, System.StringComparison.OrdinalIgnoreCase);

            // Assert: Weitere typische Elemente (passe ggf. an)
            Assert.Contains("Start", cut.Markup, System.StringComparison.OrdinalIgnoreCase);
        }
    }
}
