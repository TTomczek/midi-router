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

Für ausgewählte Eingabegeräte routet die Anwendung MIDI-Nachrichten über einen
gemeinsamen virtuellen MIDI-Endpunkt. Channel-Voice-Nachrichten werden auf den
pro Gerät konfigurierten Ausgabekanal (1-16) geändert; der interne Wertebereich
ist 0-15. Nachrichten vom virtuellen Endpunkt werden anhand dieses Kanals an das
passende Gerät zurückgesendet.

Die Geräte-Enumeration ist über `IMidiInputDeviceProvider` abstrahiert. Dadurch
kann die UI-Logik ohne physische MIDI-Hardware getestet werden.

## Profile

Die aktive Konfiguration wird links neben dem Einstellungsmenü ausgewählt. Profile
werden getrennt unter `%LOCALAPPDATA%\MIDI Router\Profiles` gespeichert und enthalten
Geräteauswahl, Kanalzuordnung und Änderungszeitpunkt. Der erste Eintrag erstellt ein
leeres Profil; vorhandene Einträge können per Doppelklick umbenannt und über die
Minus-Schaltfläche (außer beim letzten Profil) nach Bestätigung gelöscht werden.
Gleichnamige Profile bleiben intern getrennt und erhalten in der Auswahl fortlaufende
Anzeigenamen wie `Studio`, `Studio (2)` und `Studio (3)`.

## Protokollierung

Die Anwendung schreibt Aktionen und MIDI-Geräteereignisse dauerhaft nach
`%LOCALAPPDATA%\MIDI Router\Logs\midi-router.log`. Bei einer Dateigröße von
5 MB wird rotiert; die fünf vorherigen Dateien bleiben als `.1` bis `.5`
erhalten.

Voraussetzung für die Geräteerkennung:

- Windows 10 (Build 19041 oder neuer) mit verfügbarer Windows MIDI Services-Laufzeit.

## Entwicklung

Voraussetzungen:

- Windows
- .NET 10 SDK

Build und Tests werden aus dem Repository-Stamm ausgeführt:

```powershell
dotnet build
dotnet test
```
