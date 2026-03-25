# Changelog

Alle nennenswerten Änderungen an diesem Projekt werden in dieser Datei dokumentiert.

Die Struktur orientiert sich an *Keep a Changelog* und verwendet semantisch lesbare Versionsstände.

## [Unreleased]

### Geplant
- Weiterer Ausbau der 1.1-Linie auf Basis des verabschiedeten Umsetzungspakets.
- Pfad-Workflow mit Enter-Navigation und sauberer Aktualisierung des aktiven Fensters.
- Copy-as-Path in Windows- und Linux-/WSL-Format.
- Start einer Shell im aktuell angezeigten Verzeichnis.
- Harmonisierung von Shortcuts, Toolbar-Erweiterungen und spätere Drag-and-Drop-Verbesserungen.

## [1.1.0-alpha.1] - 2026-03-25

### Added
- Produktmetadaten als eigene Grundlage für Name, Version und externe Verweise eingeführt.
- Neuer About-Dialog mit Versionsanzeige und Projekt-/SASD-Verweisen vorbereitet.
- Persistenter Settings-Speicher als Grundlage für weitere 1.1-Konfigurationsfunktionen ergänzt.
- Wiederherstellung von Fenstergröße und Fensterposition als Basis für ein konsistenteres Sitzungsverhalten ergänzt.
- Schutz gegen ungültige bzw. nicht sichtbare Startpositionen des Hauptfensters vorgesehen.

### Changed
- Produktauftritt in Richtung SASD-Branding verschoben.
- Hauptfenster-Titel von einer generischen Darstellung auf einen stabileren Produktbezug vorbereitet.
- Anwendungsstart für den 1.1-Grundaufbau bereinigt.
- Projektmetadaten für einen ersten 1.1-Alpha-Stand fortgeschrieben.

### Notes
- Dieser Stand ist bewusst ein Fundament-Release und noch keine vollständige 1.1-Ausbaustufe.
- Die sichtbaren Produktverbesserungen bilden die Grundlage für die nachfolgenden Tickets zu Navigation, Pfadfunktionen, Shell-Integration und Bedienkomfort.

## [1.0.0] - 2026-03-25

### Baseline
- Bestehender Retro-NT-File-Manager-Kern mit MDI-Hauptfenster, Baum-/Listenansicht, Laufwerksauswahl, Pfadzeile, Ansichten, klassischen Dateioperationen, Eigenschaften-Dialog, Statusleiste und Kontextmenü als dokumentierte Ausgangsbasis festgehalten.
- Projekt als Windows-Forms-Anwendung auf .NET Framework 4.8 für Visual Studio 2022 dokumentiert.
- Funktions-, Benutzer-, Administrator-, Gap-, Lizenz- und Planungsdokumentation für den Ausgangsstand erstellt.

---

## Hinweise zur Pflege

- Neue Änderungen zuerst unter **[Unreleased]** eintragen.
- Beim Erstellen eines Releases die Einträge in einen neuen Versionsblock verschieben.
- Nur fachlich sichtbare oder architektonisch wichtige Änderungen aufnehmen.
- Reine Kosmetik ohne Relevanz für Nutzer, Wartung oder Architektur nicht unnötig aufblasen.
