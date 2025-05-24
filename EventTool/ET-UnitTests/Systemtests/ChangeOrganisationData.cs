using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET_UnitTests.Systemtests
{
    public class ChangeOrganisationData
    {
        [Fact]
        public async Task Login_And_EditOrganizationData_ShouldCompleteSuccessfully()
        {
            using var playwright = await Playwright.CreateAsync();
            var browser = await playwright.Chromium.LaunchAsync(
                              new() { Headless = false, SlowMo = 150 });   // Headless=false für Debug

            var context = await browser.NewContextAsync();
            var page = await context.NewPageAsync();

            /* ---------- LOGIN ---------- */

            await page.GotoAsync("https://localhost:7210/");

            await page.FillAsync(":nth-match(input, 1)", "admin@demo.org");
            await page.FillAsync(":nth-match(input, 2)", "demo");
            await page.ClickAsync("button:has-text('Anmelden')");


            await page.WaitForURLAsync(url => !url.ToString().Contains("/login"), new() { Timeout = 15000 });

            /* ---------- ORGANISATIONSDATEN ---------- */
            await page.GotoAsync("https://localhost:7210/Organisationsdaten");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // 3) Formularelemente wieder über Label ansprechen
            await page.GetByLabel("Organisationsname").FillAsync("Meine Firma 2.0");
            await page.GetByLabel("Beschreibung").FillAsync("Automatischer Test-Update ✔");
            await page.GetByLabel("Domain").FillAsync("demo.org");

            await page.GetByRole(AriaRole.Button, new() { Name = "Änderungen Speichern" }).ClickAsync();

            // 4) Toast/Popup abwarten und verifizieren
            await page.WaitForSelectorAsync("text=Änderungen gespeichert");
            Assert.True(await page.IsVisibleAsync("text=Änderungen gespeichert"));
        }


    }
}
