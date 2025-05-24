## 📋 Zusammenfassung: Story 5.1, Struktur-Entscheidungen & Styleguide-Vorlage

### 🧩 Status Quo: Aktuelle Aufgabenstellung (Stand 23.05.2025)

* Ziel: Story 5.1 „Benutzerdaten bearbeiten" voll funktionsfähig und sauber umgesetzt als **Vorlage für alle weiteren Stories**.
* Fokus liegt aktuell auf: **Funktionalität** bis zur Endabnahme (in 4 Tagen).
* Refactoring und Style-Anpassung folgen danach (4 Wochen Pufferzeit).

---

## ✅ Umsetzung Story 5.1 (Benutzerdaten bearbeiten)

### 🔄 Beteiligte Komponenten (Frontend bis Backend):

* **View:** `UserEdit.razor`
* **ViewModel:** `UserEditViewModel.cs` (ausgelagert)
* **ApiClient:** `UserApi.cs` + `IUserApi.cs`
* **DTO:** `UserDto.cs`
* **Model:** `User.cs`
* **Mapper:** `UserMapper.cs` in `Services/Mapping`
* **Service:** `UserService.cs` + `IUserService.cs`
* **Repository:** `UserRepository.cs` + `IUserRepository.cs`
* **Auth:** `JwtClaimHelper.cs` + `JwtAuthenticationStateProvider.cs`

### 🔧 Besondere Logik:

* Passwort wird nur gespeichert, wenn ein neues übermittelt wird.
* Benutzer-ID wird über JWT `sub`-Claim ermittelt.
* Authentifizierungsstatus wird per `AuthenticationStateProvider` geprüft.

---

## 🧱 Architekturempfehlungen (als Styleguide-Grundlage)

### 🔗 1. **Trennung von Zuständigkeiten (Single Responsibility Principle)**

| Schicht          | Aufgabe                                             |
| ---------------- | --------------------------------------------------- |
| Razor-Komponente | UI, Interaktionen, Anzeige                          |
| ViewModel        | Validierung & Datenbindung                          |
| ApiClient        | HTTP-Kommunikation (kein `HttpClient` direkt in UI) |
| Service          | Geschäftslogik                                      |
| Repository       | DB-Zugriffe via Dapper                              |
| Mapper           | Konvertierung DTO <-> Model                         |

### 🔐 2. **JWT & Authentifizierung (Frontend)**

* `JwtClaimHelper` zentralisiert Claim-Zugriffe → bevorzugt `GetUserIdAsync(...)`
* Claims werden über `AuthenticationStateProvider` abgerufen
* AuthHeaderHandler hängt automatisch `Bearer`-Token an `HttpClient`

### 🌐 3. **HttpClient + Factory + Named Clients**

* `HttpClientFactory` wird genutzt für **named client** `"ApiClient"`
* `Program.cs` registriert \*\*default \*\*\`\`, damit `@inject HttpClient` funktioniert
* API-Klassen (z. B. `UserApi`) nutzen DI, um AuthProvider & Client zu kombinieren

```csharp
public UserApi(HttpClient client, AuthenticationStateProvider auth)
```

### ✅ 4. **Warum **\`\`** beibehalten?**

* Trennung UI ↔ API-Logik
* testbar durch Interface
* übersichtlicher Code
* Wiederverwendbarkeit

---

## 🔮 Ausblick Refactoring (nach Endabnahme)

* **Mapping zentralisieren:** AutoMapper oder eigene `MapperRegistry`
* **API-Clients vereinheitlichen:** `BaseApi<T>` möglich
* **DTO ↔ Model konsequent trennen:** keine Verwendung von DTOs im Backend
* **Claims, Tokens, AuthHeader in eigene Helper-Bibliothek auslagern**
* **Coding-Styleguide fürs Team ableiten (Namenskonventionen, Struktur, DI-Strategien)**

---

## 📎 Hinweise zum Weiterarbeiten

* Die Codebasis ist modular und kann unabhängig geladen werden
* Bei Bedarf: alle API-Clients und Services analog zu Story 5.1 aufbauen
* Bei Bugs: Netzwerkanalyse/Fehlermeldung ansehen → vermutlich Auth- oder Routingproblem
* Alle bisherigen Entscheidungen sind dokumentiert und für Wiederverwendung geeignet ✅

---

> **Erstellt am 23.05.2025 – als Basis für Endabnahme und Refactoringphase**