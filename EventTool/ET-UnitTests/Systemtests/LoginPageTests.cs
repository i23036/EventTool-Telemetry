using Microsoft.Playwright;
using System.Threading.Tasks;
using Xunit;

namespace ET_UnitTests.Systemtests
{
    public class LoginPageTests
    {
        [Fact]
        public async Task Login_ShouldRedirectToHomePage()
        {
            using var playwright = await Playwright.CreateAsync();
            var browser = await playwright.Chromium.LaunchAsync(new() { Headless = false, SlowMo = 500 });
            var page = await browser.NewPageAsync();

            await page.GotoAsync("https://localhost:7210/");
            await page.WaitForSelectorAsync("text=Login");
            await page.ClickAsync("text=Login");

            await page.WaitForSelectorAsync("input[placeholder='E-Mail']"); // <- hier kommt’s zum Timeout
            await page.FillAsync("input[placeholder='E-Mail']", "test@example.com");
            await page.FillAsync("input[placeholder='Passwort']", "password123");
            await page.ClickAsync("button:has-text('Anmelden')");

            await page.WaitForURLAsync("**/home");
            Assert.Contains("/home", page.Url);
        }


    }
}
