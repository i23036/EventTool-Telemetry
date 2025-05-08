using Microsoft.Playwright;
using System.Threading.Tasks;
using Xunit;

namespace ET_UnitTests.Systemtests
{
    public class HomePageTests
    {
        [Fact]
        public async Task HomePage_ShouldLoadSuccessfully()
        {
            // Arrange
            using var playwright = await Playwright.CreateAsync();
            var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
            var page = await browser.NewPageAsync();

            // Act
            await page.GotoAsync("https://localhost:7210/");

            // Assert
            var content = await page.ContentAsync();
            Assert.False(string.IsNullOrEmpty(content), "Die Startseite sollte erfolgreich geladen werden.");
        }
    }
}
