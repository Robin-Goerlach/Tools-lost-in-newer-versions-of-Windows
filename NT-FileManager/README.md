# SASD Retro NT File Manager

Der **SASD Retro NT File Manager** ist ein Windows-Dateimanager in **C# / WinForms** fuer **Visual Studio 2022** und **.NET Framework 4.8**. Das Projekt greift die Arbeitsweise und Anmutung des klassischen Windows-NT-4.0-File-Managers auf, wird aber als eigenstaendige SASD-Anwendung weiterentwickelt.

Der aktuelle oeffentliche Repository-Stand zeigt noch an mehreren Stellen den aelteren Namen **"Retro NT File Manager"** bzw. generische Texte wie **"File Manager"**. Dieses README ist als bereinigte und zukunftsfeste Fassung fuer den naechsten konsistenten Dokumentationsstand gedacht.

## Projektziel

Ziel des Projekts ist kein pixelgenauer Museums-Nachbau, sondern ein **retro-inspirierter, produktiver Dateimanager** mit klassischer Bedienlogik und schrittweiser Modernisierung. Die Anwendung soll bewusst leichtgewichtig bleiben, aber gleichzeitig fuer SASD typische Eigenschaften bekommen:

- klare Produktidentitaet
- saubere Bedienlogik
- nachvollziehbare Weiterentwicklung
- professionelle Dokumentation
- spaetere Erweiterbarkeit fuer Power-User-Funktionen

## Technologie-Stack

- **Sprache:** C#
- **UI-Technologie:** Windows Forms
- **Zielplattform:** .NET Framework 4.8
- **IDE:** Visual Studio 2022 unter Windows

## Projektstruktur

```text
NT-FileManager/
|-- NT-FileManager.sln
|-- README.md
`-- NT-FileManager/
    |-- NT-FileManager.csproj
    |-- MainForm.cs
    |-- DirectoryWindowForm.cs
    |-- FileSystemOperations.cs
    |-- PropertiesDialog.cs
    |-- ShellIconHelper.cs
    `-- weitere WinForms-Klassen und Ressourcen
```

## Bereits umgesetzte Kernfunktionen

Der derzeit sichtbare Kernumfang des Projekts umfasst insbesondere:

- klassisches **MDI-Hauptfenster** mit Menueleiste, Toolbar und Statusleiste
- **linke Baumansicht** und **rechte Dateiliste** in den Verzeichnisfenstern
- **Laufwerksauswahl** und **Pfadzeile**
- Ansichten fuer **grosse Symbole**, **kleine Symbole**, **Liste** und **Details**
- Oeffnen von Dateien und Ordnern
- **Kopieren**, **Verschieben**, **Loeschen**, **Umbenennen**
- **Neuer Ordner**
- **Eigenschaften-Dialog**
- **Sortierung per Spaltenklick**
- **Statusanzeige**
- **Kontextmenue**
- globale Tastaturkuerzel fuer zentrale Datei- und Ansichtsfunktionen

## Bekannte Grenzen des aktuellen Stands

Das Projekt ist bereits benutzbar, bildet aber noch nicht alle historischen oder modern erwartbaren Funktionen ab. Aktuell gelten insbesondere diese Grenzen:

- keine vollstaendige Replik aller historischen Spezialfaelle des Original-File-Managers
- Netzwerkfunktionen nur eingeschraenkt bzw. noch nicht umfassend modernisiert
- keine voll ausgebaute Suche nach modernem Muster
- keine umfassende Shell-/Terminal-Integration im aktuellen Hauptstand
- keine vollstaendige Mehrsprachigkeit
- keine produktionsreife Theme-/Template-Schicht

## Richtung der Version 1.1

Fuer **Version 1.1** ist bewusst keine unkontrollierte Vollerweiterung geplant. Stattdessen wird das Projekt in kleinen, pruefbaren Schritten modernisiert. Der derzeit priorisierte Ausbau konzentriert sich auf alltagsnahe und risikoarme Verbesserungen, darunter insbesondere:

- **SASD-Branding** statt generischer Produktbezeichnungen
- verbesserter **About-/Produktinfo-Dialog**
- Grundlage fuer **persistente Einstellungen**
- **Fensterzustand und Position** sauber wiederherstellen
- robusteres Verhalten bei **off-screen gestarteten Fenstern**
- verbesserte **Pfad- und Shell-Workflows** in spaeteren 1.1-Schritten

## Start in Visual Studio

1. `NT-FileManager.sln` in **Visual Studio 2022** oeffnen.
2. NuGet-/Projektwiederherstellung ausfuehren, falls Visual Studio dazu auffordert.
3. Projekt **builden**.
4. Die Anwendung als WinForms-Desktopanwendung starten.

## Hinweise zur Entwicklung

- Die Anwendung ist bewusst **retro-inspiriert**, aber kein Copy-and-Paste-Ableger des Microsoft-Winfile-Codes.
- Funktionsideen aus historischen oder modernen Dateimanagern werden nach Moeglichkeit **eigenstaendig** und **SASD-konform** umgesetzt.
- Neue Features sollen nur dann aufgenommen werden, wenn sie in den vorgesehenen Release-Scope passen und die Bedienlogik nicht unnoetig verkomplizieren.

## Repository-Empfehlungen

Fuer einen professionellen oeffentlichen Eindruck empfiehlt sich mittelfristig zusaetzlich:

- ein `CHANGELOG.md`
- GitHub **Releases** fuer herunterladbare Builds statt EXE-Dateien direkt im Repository
- spaeter optional `SECURITY.md`, `CONTRIBUTING.md` und weiterfuehrende Doku unter `docs/`

## Lizenz und Herkunft

Bitte die Lizenzdatei im Repository beachten. Bei der Weiterentwicklung gilt: **Feature-Inspiration ist nicht dasselbe wie Codeuebernahme**. Fuer uebernommene Drittbestandteile, Assets oder Texte muessen die jeweiligen Lizenz- und Herkunftshinweise sauber eingehalten werden.

## Status

Diese README-Fassung ist als **konsolidierte Projektbeschreibung fuer den naechsten bereinigten Dokumentationsstand** gedacht. Sie ersetzt die aktuell oeffentlich noch sichtbaren alten Dateinamen und Bezeichnungen durch eine konsistentere, zukunftsfaehige Beschreibung.
