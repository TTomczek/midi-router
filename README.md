# MIDI Router

MIDI Router ist eine leichtgewichtige WPF-Anwendung für Windows und .NET 8.
Beim Start werden alle verfügbaren MIDI-Eingabegeräte mit ihrem Namen angezeigt.
Beim Minimieren wird das Fenster in den Windows-Infobereich (Tray) verschoben.

## MIDI-Eingabegeräte

Die Übersicht verwendet Windows MIDI Services (Windows Runtime MIDI-Geräteerkennung) und zeigt:

- den Namen jedes verfügbaren MIDI-Eingabegeräts,
- ob das Gerät den nativen MIDI-1- oder MIDI-2-Endpunkt verwendet,
- die Anzahl der gefundenen Geräte,
- einen Status für leere Listen oder Enumerationsfehler.

Mit **Aktualisieren** kann die Liste nach dem Anschließen oder Entfernen eines
Geräts erneut eingelesen werden. Eine automatische Hot-Plug-Erkennung ist noch
nicht Bestandteil der Anwendung.

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
- .NET 8 SDK

Build und Tests werden aus dem Repository-Stamm ausgeführt:

```powershell
dotnet build
dotnet test
```
