# SASD - Filemanager 1.1 Grundausbau

Dieses Paket enthält einen ersten umsetzbaren 1.1-Code-Stand für die Basisfeatures.

## Enthaltene Änderungen

- feste Haupttitelzeile `SASD - Filemanager`
- About-Dialog mit Versionsanzeige und Links zu SASD und Repository
- SDK-Projektdatei mit Versionsmetadaten `1.1.0-alpha.1`
- persistenter Settings-Speicher unter `%LocalAppData%\SASD\FileManager\settings.xml`
- Wiederherstellung von Fenstergröße, Position und Zustand
- Off-screen-Rettung bei ungültigen Monitor-Konstellationen
- `Application.EnableVisualStyles()` im Programmeinstieg

## Noch bewusst nicht enthalten

- neues Startleisten-/EXE-Icon
- Pfadnavigation per Enter in der Pfadzeile
- Copy as Path / Copy as Linux Path
- Terminal/CMD im aktuellen Verzeichnis
- Drag & Drop zwischen Fenstern

## Nächster sinnvoller Schritt

1. Code in einen Branch wie `feature/1.1-foundation-branding-settings` übernehmen.
2. In Visual Studio 2022 kompilieren und manuell testen.
3. Danach Pfad- und Shell-Workflow als nächste 1.1-Welle implementieren.
