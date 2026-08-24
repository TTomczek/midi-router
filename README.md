# MIDI Router

MIDI Router ist eine leichtgewichtige WPF-Anwendung für Windows und .NET 10.
Beim Start werden alle verfügbaren MIDI-Eingabegeräte mit ihrem Namen angezeigt.

## Windows-Infobereich

Über das Einstellungsmenü kann das Minimieren in den Windows-Infobereich aktiviert
oder deaktiviert werden. Ist die Option aktiviert, wird das Fenster beim Minimieren
aus der Taskleiste entfernt und bleibt über das Tray-Symbol erreichbar. Ein Linksklick
auf das Symbol stellt das Fenster wieder her; ein Rechtsklick bietet eine Option zum
Beenden der Anwendung. Ist die Option deaktiviert, wird das Fenster normal in der
Taskleiste minimiert. Die Auswahl wird dauerhaft gespeichert.

## MIDI-Eingabegeräte

Die Übersicht verwendet Windows MIDI Services (Windows Runtime MIDI-Geräteerkennung) und zeigt:

- den Namen jedes verfügbaren MIDI-Eingabegeräts,
- ob das Gerät den nativen MIDI-1- oder MIDI-2-Endpunkt verwendet,
- die Anzahl der gefundenen Geräte,
- einen Status für leere Listen oder Enumerationsfehler.

Die Liste wird nach dem Anschließen oder Entfernen eines Geräts automatisch
aktualisiert. Die Überwachung läuft im Hintergrund und wird beim Beenden sauber
gestoppt.

Geräte können durch Anklicken ihrer Zeile für die spätere Verarbeitung ausgewählt
werden. Ein erneuter Klick hebt die Auswahl auf; ausgewählte Zeilen werden
hervorgehoben. Die Auswahl wird anhand der eindeutigen Geräte-ID gespeichert und
nach einem Neustart sowie beim Wiederverbinden desselben Geräts wiederhergestellt.

Für jedes gefundene Eingabegerät erstellt die Anwendung einen virtuellen MIDI-
Ausgang mit dem Namen `MIDI Router - <Gerätename>`. Eingehende MIDI-1- und
MIDI-2-Channel-Voice-Nachrichten werden auf den pro Gerät ausgewählten
Ausgabekanal (1-16) geändert und an diesen Ausgang weitergeleitet.

Die Geräte-Enumeration ist über `IMidiInputDeviceProvider` abstrahiert. Dadurch
kann die UI-Logik ohne physische MIDI-Hardware getestet werden.

Voraussetzung für die Geräteerkennung:

- Windows 11 mit verfügbarer Windows MIDI Services-Laufzeit.

## Entwicklung

Voraussetzungen:

- Windows
- .NET 10 SDK

Build und Tests werden aus dem Repository-Stamm ausgeführt:

```powershell
dotnet build
dotnet test
```
