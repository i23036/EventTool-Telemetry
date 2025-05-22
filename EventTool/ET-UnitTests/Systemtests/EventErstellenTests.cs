using Microsoft.Playwright;
using System.Threading.Tasks;
using Xunit;

namespace ET_UnitTests.Systemtests
{
    public class CreatingEventTests
    {
        /// <summary>
        /// Testet die Erstellung eines Events über die Benutzeroberfläche.
        /// </summary>
        /// <remarks>
        /// Dieser Test geht davon aus, dass der Benutzer bereits existiert
        /// </remarks>
        /// <returns></returns>
    
        [Fact]
        public async Task EventErstellen_SollteErfolgreichSein()
        {
            using var playwright = await Playwright.CreateAsync();
            var browser = await playwright.Chromium.LaunchAsync(new() { Headless = false, SlowMo = 500 });
            var page = await browser.NewPageAsync();

            // Login
            await page.GotoAsync("https://localhost:7210/");
            await page.FillAsync(":nth-match(input, 1)", "admin@demo.org");
            await page.FillAsync(":nth-match(input, 2)", "demo");
            await page.ClickAsync("button:has-text('Anmelden')");

            // "Events"-Reiter auswählen
            await page.ClickAsync(".mud-tab:has-text('Events')");

            // Auf "Event erstellen" klicken
            await page.ClickAsync("button:has-text('Event erstellen')");

            // Felder per Label füllen
            await page.GetByLabel("Eventname").FillAsync("Testevent");
            await page.GetByLabel("Eventtyp").FillAsync("Workshop");
            await page.GetByLabel("Beschreibung").FillAsync("Dies ist ein Testevent.");
            await page.GetByLabel("Ansprechpartner").FillAsync("Max Mustermann");

            // Datumfelder (MudDatePicker: erst Feld klicken, dann Datum eingeben)
            await page.GetByLabel("Startdatum").ClickAsync();
            await page.Keyboard.TypeAsync("01.01.2025");
            await page.Keyboard.PressAsync("Enter");

            await page.GetByLabel("Enddatum").ClickAsync();
            await page.Keyboard.TypeAsync("02.01.2025");
            await page.Keyboard.PressAsync("Enter");

            await page.GetByLabel("Minimale Teilnehmer").FillAsync("1");
            await page.GetByLabel("Maximale Teilnehmer").FillAsync("10");

            await page.GetByLabel("Anmeldebeginn").ClickAsync();
            await page.Keyboard.TypeAsync("01.01.2025");
            await page.Keyboard.PressAsync("Enter");

            await page.GetByLabel("Anmeldefrist").ClickAsync();
            await page.Keyboard.TypeAsync("02.01.2025");
            await page.Keyboard.PressAsync("Enter");

            // Status auswählen (MudSelect)
            await page.GetByLabel("Status").ClickAsync();
            await page.ClickAsync("div.mud-list-item:has-text('Offen')");

            // Speichern
            await page.ClickAsync("button:has-text('SPEICHERN')");

            // Optional: Erfolg prüfen
            // await page.WaitForSelectorAsync("text=Event wurde erfolgreich erstellt");
        }
    }
}
