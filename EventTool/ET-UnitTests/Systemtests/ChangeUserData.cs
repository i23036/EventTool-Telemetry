using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET_UnitTests.Systemtests
{
    public class ChangeUserData
    {

        [Fact]
        public async Task NutzerdatenBearbeiten_SollteErfolgreichSein()
        {
            using var playwright = await Playwright.CreateAsync();
            var browser = await playwright.Chromium.LaunchAsync(new() { Headless = false, SlowMo = 300 });
            var page = await browser.NewPageAsync();

            // ---------- LOGIN ----------
            await page.GotoAsync("https://localhost:7210/");
            await page.FillAsync(":nth-match(input, 1)", "admin@demo.org");
            await page.FillAsync(":nth-match(input, 2)", "demo");
            await page.ClickAsync("button:has-text('Anmelden')");

            // ---------- MENÜ ÖFFNEN ----------
            await page.Locator(".mud-avatar").ClickAsync();
            await page.ClickAsync("button:has-text('Nutzerdaten bearbeiten')");

            // ---------- FELDER ----------
            await page.GetByLabel("Vorname").FillAsync("Max");
            await page.GetByLabel("Nachname").FillAsync("Mustermann");
            await page.GetByLabel("Passwort").Nth(0).FillAsync("Test1234!");
            await page.GetByLabel("Passwort").Nth(1).FillAsync("Test1234!"); // Wiederholung

            // ---------- SPEICHERN ----------
            await page.ClickAsync("button:has-text('Änderungen speichern')");

            // ---------- POPUP-KONTROLLE ----------
            var popup = page.Locator("text=Änderungen gespeichert");
            await popup.WaitForAsync(new() { Timeout = 3000 });
            Assert.True(await popup.IsVisibleAsync());
        }

    }
}
