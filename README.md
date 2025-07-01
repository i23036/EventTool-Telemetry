# EventTool-Telemetry

🔬 **Telemetrie – Umsetzung aktueller Best Practices in .NET**

Dies ist ein **persönlicher Fork** des Projekts [EventTool](https://github.com/BitWorks-ET/Event-Tool), das im Rahmen des Moduls *Software Engineering I* an der DHBW Stuttgart entstanden ist.

Dieser Fork dient als **Grundlage für die Projektarbeit (T2000)** im Studiengang Angewandte Informatik mit dem Thema:

> **„Telemetrie – Umsetzung aktueller Best Practices in .NET“**

---

## 🎯 Zielsetzung der Arbeit

Ziel ist es, die Vorteile und Herausforderungen moderner Telemetrie-Ansätze praxisnah zu untersuchen. Hierzu wird der OpenTelemetry-Techstack exemplarisch in dieses Softwareprojekt integriert. Neben der Implementierung von Tracing, Logging und Metriken werden auch Tools zur Erfassung, Speicherung, Analyse und Visualisierung einbezogen (z. B. Jaeger, Prometheus, Grafana, Loki).

Die Ergebnisse sollen insbesondere .NET-Entwicklern eine **praxisnahe Einführung in Telemetrie** bieten.

---

## 🛠️ Geplantes Vorgehen

- Literaturrecherche zu Telemetrie & Observability
- Auswahl geeigneter Tools (OpenTelemetry, Jaeger, Prometheus, u. a.)
- Integration von Telemetrie in Backend & ggf. Frontend
- Messung und Analyse von Laufzeitdaten
- Auswertung und Dokumentation der Ergebnisse

---

## 📂 Projektstruktur

```plaintext
source/
└── EventTool/
    ├── ET-Backend/              # ASP.NET Core Backend – Telemetrieintegration geplant
    ├── ET-Frontend/             # Blazor WebAssembly – optional instrumentierbar
    ├── ET-UnitTests/            # Unittests zur Validierung und Ergänzung
    ├── ET-TelemetryPlayground/ # Geplantes Projekt für isolierte Telemetrie-Experimente
    └── TelemetrySetup/          # Gemeinsame OpenTelemetry-Konfiguration & DI-Erweiterungen
```
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
5. **Datenbank** – Speicherung in SQLite (Dev) und FullSQL (Prod)
