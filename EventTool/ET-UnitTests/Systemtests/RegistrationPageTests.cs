using Microsoft.Playwright;
using System.Threading.Tasks;
using Xunit;

namespace ET_UnitTests
{
    public class RegistrationPageTests
    {
        [Fact]
        public async Task Register_ShouldCompleteSuccessfully()
        {
            using var playwright = await Playwright.CreateAsync();
            var browser = await playwright.Chromium.LaunchAsync(new() { Headless = false, SlowMo = 300 });
            var page = await browser.NewPageAsync();

            await page.GotoAsync("https://localhost:7210/register");

            await page.FillAsync(":nth-match(input, 1)", "Max");
            await page.FillAsync(":nth-match(input, 2)", "Mustermann");
            await page.FillAsync(":nth-match(input, 3)", "max@example.com");
            await page.FillAsync(":nth-match(input, 4)", "pass1234");
            await page.FillAsync(":nth-match(input, 5)", "pass1234");

            await page.ClickAsync("button:has-text('Registrieren')");

            // Optional: Warten auf Erfolg oder URL-Wechsel
            // await page.WaitForURLAsync("**/login");
        }

    }
}
