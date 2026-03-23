# Retro NT File Manager

Dieses Paket enthält ein Visual-Studio-2022-Projekt in C# / WinForms, das den klassischen Windows-NT-4.0-File-Manager gestalterisch und funktional nachbildet.

## Inhalt

- `RetroNtFileManager.sln` – Visual-Studio-Lösung
- `RetroNtFileManager/RetroNtFileManager.csproj` – Projektdatei
- WinForms-Quellcode für MDI-Hauptfenster, Verzeichnisfenster, Dateiansicht, Dateioperationen und Eigenschaften-Dialog

## Umgesetzte Funktionen

- klassisches MDI-Hauptfenster mit Menüs und Toolbar
- linke Baumansicht, rechte Dateiliste
- Laufwerksauswahl und Pfadzeile
- Ansichten: große Symbole, kleine Symbole, Liste, Details
- Öffnen von Dateien und Ordnern
- Kopieren, Verschieben, Löschen, Umbenennen
- Neuer Ordner
- Eigenschaften-Dialog
- Sortierung per Spaltenklick
- Statusleisten-Anzeige
- Kontextmenü

## Hinweise

- Zielplattform ist **Visual Studio 2022 auf Windows**.
- Das Projekt verwendet **.NET Framework 4.8** und **Windows Forms**.
- Die Oberfläche ist bewusst retro gehalten, aber keine bitgenaue 1:1-Replik aller historischen Spezialfälle.
- Netzwerkfunktionen, Drucker-/Freigabe-Verwaltung und einige sehr spezielle Alt-Funktionen des historischen Originals sind in dieser Version nicht vollständig nachgebaut.

## Start

1. Lösung `RetroNtFileManager.sln` in Visual Studio 2022 öffnen.
2. Wiederherstellen/Build ausführen.
3. Projekt starten.

