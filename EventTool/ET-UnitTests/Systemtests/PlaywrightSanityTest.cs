using Microsoft.Playwright;
using System.Threading.Tasks;
using Xunit;

namespace ET_UnitTests.Systemtests
{
    public class PlaywrightSanityTest
    {
        [Fact]
        public async Task Playwright_ShouldLaunchBrowserSuccessfully()
        {
            // Arrange
            using var playwright = await Playwright.CreateAsync();
            var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
            var page = await browser.NewPageAsync();

            // Act
            await page.GotoAsync("about:blank");

            // Assert
            var content = await page.ContentAsync();
            Assert.False(string.IsNullOrEmpty(content), "Die Seite sollte erfolgreich geladen werden.");
        }
    }
}
