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
        [Fact]
        public async Task EventErstellen_SollteErfolgreichSein()
        {
            using var playwright = await Playwright.CreateAsync();
            var browser = await playwright.Chromium.LaunchAsync(new() { Headless = false, SlowMo = 500 });
            var page = await browser.NewPageAsync();

            // ---------- LOGIN ----------
            await page.GotoAsync("https://localhost:7210/");
            await page.FillAsync(":nth-match(input, 1)", "admin@demo.org");
            await page.FillAsync(":nth-match(input, 2)", "demo");
            await page.ClickAsync("button:has-text('Anmelden')");

            // ---------- EVENT ERSTELLEN ----------
            await page.ClickAsync(".mud-tab:has-text('Events')");
            await page.ClickAsync("button:has-text('Event erstellen')");

            // ---------- FORMULAR AUSFÜLLEN ----------
            await page.GetByLabel("Eventname").FillAsync("Testevent");
            await page.GetByLabel("Eventtyp").FillAsync("Workshop");
            await page.GetByLabel("Beschreibung").FillAsync("Dies ist ein Testevent.");
            await page.GetByLabel("Ansprechpartner").FillAsync("Max Mustermann");

            await PickDateAsync(page, "Startdatum", 2025, "Januar");
            await PickDateAsync(page, "Enddatum", 2025, "Januar");
            await PickDateAsync(page, "Anmeldebeginn", 2025, "Januar");
            await PickDateAsync(page, "Anmeldefrist", 2025, "Januar");

            await page.GetByLabel("Minimale Teilnehmer").FillAsync("1");
            await page.GetByLabel("Maximale Teilnehmer").FillAsync("10");

            await page.GetByLabel("Status").ClickAsync();
            await page.ClickAsync("div.mud-list-item:has-text('Offen')");

            await page.ClickAsync("button:has-text('Speichern')");
        }

        /// <summary>
        /// Wählt im MudDatePicker das gewünschte Jahr + Monat und klickt dann den ersten wählbaren Tag.
        /// </summary>
        private static async Task PickDateAsync(IPage page, string label, int year, string monthNameGer)
        {
            var picker = page.GetByLabel(label);
            await picker.ClickAsync();  // öffnet DatePicker

            var calendar = page.Locator(".mud-popover-open");

            // 1) Jahr öffnen und wählen
            var yearHeader = calendar.Locator("button:has-text('" + year + "')").First;
            await yearHeader.ClickAsync();

            var yearEntry = calendar.Locator("div:has-text('" + year + "')").First;
            await yearEntry.ClickAsync();

            // 2) Monat auswählen (falls sichtbar)
            var monthButton = calendar.GetByRole(AriaRole.Button, new() { Name = monthNameGer });
            if (await monthButton.IsVisibleAsync())
                await monthButton.ClickAsync();

            // 3) Ersten verfügbaren Tag auswählen
            var dayCell = calendar.Locator("button.mud-picker-calendar-day:not([disabled])").First;
            await dayCell.ClickAsync();
        }
    }
}
