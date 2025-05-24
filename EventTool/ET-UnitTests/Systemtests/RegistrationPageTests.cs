using Microsoft.Playwright;
using System.Threading.Tasks;
using Xunit;

namespace ET_UnitTests.Systemtests
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
            await page.FillAsync(":nth-match(input, 3)", "max@demo.org");
            await page.FillAsync(":nth-match(input, 4)", "pass1234");
            await page.FillAsync(":nth-match(input, 5)", "pass1234");

            await page.ClickAsync("button:has-text('Registrieren')");

            // Warte auf die Weiterleitung zur Login-Seite
            await page.WaitForURLAsync("**/login");

            // Optional: Prüfe, ob die URL wirklich die Login-Seite ist
            Assert.Contains("/login", page.Url);
        }
        /// <summary>
        /// Erwartet, dass die Registrierung NICHT erfolgreich ist und
        /// dass eine passende Fehlermeldung angezeigt wird.
        /// </summary>
        [Theory]
        //                firstName,   lastName,     eMail,                       password, repPassword,                     erwarteteFehlerMsg
        [InlineData("", "Mustermann", "max@NotRegisteredDomain", "pass1234", "pass1234", "Vorname ist erforderlich.")]
        [InlineData("Max", "", "max@NotRegisteredDomain", "pass1234", "pass1234", "Nachname ist erforderlich.")]
        [InlineData("Max", "Mustermann", "", "pass1234", "pass1234", "E-Mail ist erforderlich.")]
        [InlineData("Max", "Mustermann", "falscheMail", "pass1234", "pass1234", "Ungültige E-Mail-Adresse.")]
        [InlineData("Max", "Mustermann", "max@NotRegisteredDomain", "123", "123", "Passwort muss mindestens 6 Zeichen lang sein.")]
        [InlineData("Max", "Mustermann", "max@NotRegisteredDomain", "pass1234", "anders", "Passwörter stimmen nicht überein.")]
        public async Task Register_ShouldFailValidation(
            string firstName,
            string lastName,
            string email,
            string password,
            string repPassword,
            string expectedError)
        {
            using var playwright = await Playwright.CreateAsync();
            var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
            var page = await browser.NewPageAsync();

            await page.GotoAsync("https://localhost:7210/register");

            // Felder befüllen
            await page.FillAsync(":nth-match(input, 1)", firstName);
            await page.FillAsync(":nth-match(input, 2)", lastName);
            await page.FillAsync(":nth-match(input, 3)", email);
            await page.FillAsync(":nth-match(input, 4)", password);
            await page.FillAsync(":nth-match(input, 5)", repPassword);

            await page.ClickAsync("button:has-text('Registrieren')");

            // Kurz warten, damit mögliche Validierungs-UI gerendert wird
            await page.WaitForTimeoutAsync(500);

            // 1) Die URL darf sich NICHT auf /login geändert haben
            Assert.Contains("/register", page.Url);

            // 2) Die erwartete Fehlermeldung muss irgendwo im DOM auftauchen
            //    (falls du eine eigene CSS-Klasse hast, die besser gezielt werden kann – anpassen!)
            var errorVisible = await page.Locator($"text={expectedError}").IsVisibleAsync();
            Assert.True(errorVisible, $"Erwartete Fehlermeldung wurde nicht gefunden: '{expectedError}'");
        }



    }
}
