# 🎯 Event-Tool – BitWorks

Ein webbasiertes System zur Planung, Verwaltung und Durchführung von Veranstaltungen für Organisationen wie Unternehmen oder Vereine. Entwickelt im Rahmen der Veranstaltung **Software Engineering I (T3INF2003)** im 4. Semester an der DHBW.

---

## 🔍 Projektüberblick

Das **Event-Tool** unterstützt Organisationen bei der Eventverwaltung und bietet:
- Erstellung und Bearbeitung von Events
- Rollen- und Benutzerverwaltung
- Event-Vorlagen und Prozessautomatisierung
- Externe Einladung und Self-Service-Registrierung
- Teilnehmermanagement mit Upload-/Downloadbereich

Ziel ist es, organisatorische Abläufe durch automatisierte Prozesse zu entlasten.

---

## 🛠️ Technologiestack

| Komponente        | Technologie                |
|-------------------|----------------------------|
| Programmiersprache | C# (.NET 8.0)              |
| Frontend           | Blazor WebAssembly         |
| Backend            | ASP.NET Core Web API       |
| Datenbank          | SQLite                     |
| ORM                | Dapper                     |
| Authentifizierung  | JWT                        |
| Tests              | MSTest, xUnit, PlayWright  |
| Versionskontrolle  | Git (GitHub)               |
| UML                | PlantUML                   |

---

## 🧩 Architektur

Die Anwendung folgt einer mehrschichtigen Architektur:

1. **Frontend** – Blazor WebAssembly (Präsentation & Benutzerinteraktion)
2. **Controller** – REST API (Validierung & Routing)
3. **Service-Schicht** – Geschäftslogik & Prozesssteuerung
4. **Repository-Schicht** – Datenzugriff via Dapper
5. **Datenbank** – Speicherung in SQLite

---

## 🔐 Rollenmodell

- **Administrator** – verwaltet Organisationen & Plattform
- **Owner** – verwaltet Organisation, ernennt Rollen
- **Organisator** – verwaltet Events & Prozesse
- **Mitglied** – kann Events sehen & daran teilnehmen
- **Externer Gast** – kann über Einladung teilnehmen

---

## 📁 Dokumentation

---

## 👨‍💻 Projektteam

**Projektgruppe:** BitWorks  
**Projektwebsite:** https://bitworks-et.github.io/Website/  
**Repository:** https://github.com/BitWorks-ET/Event-Tool

---

## 📅 Projektzeitraum

März – Juni 2025  
Teil des Moduls Software Engineering I  
Umfang: ca. 100 Stunden pro Teilnehmer

---

## 📜 Lizenz

Dieses Projekt ist ein nicht-kommerzielles Hochschulprojekt und steht unter keiner Open-Source-Lizenz.
