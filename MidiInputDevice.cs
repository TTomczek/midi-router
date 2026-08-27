using System.Collections.ObjectModel;

namespace midi_router;

public enum MidiVersion
{
    Unknown,
    Midi1,
    Midi2
}

public enum DeviceOverviewState
{
    Loading,
    Ready,
    Empty,
    Degraded,
    Unavailable,
    Stopped
}

/// <summary>
/// Mirrors Windows.Devices.Midi2.MidiApiMode without leaking the WinRT type into
/// portable code. In <see cref="Legacy"/> or <see cref="HybridLegacy"/> mode, Windows
/// MIDI Services SDK apps cannot see MIDI 1.0 ports created by WinMM, the legacy
/// usbaudio.sys MIDI 1.0 USB driver, or vendor MIDI 1 drivers, so those physical
/// devices will not appear or send messages to this application.
/// </summary>
public enum MidiApiMode
{
    Full,
    Legacy,
    HybridLegacy
}

public sealed record MidiInputDevice(
    string EndpointDeviceId,
    string Name,
    MidiVersion Version);

public sealed record DeviceOverviewSnapshot(
    IReadOnlyList<MidiInputDevice> Devices,
    DeviceOverviewState State,
    string? StatusMessage);

public static class MidiDeviceProjection
{
    public static MidiInputDevice FromEndpoint(
        string endpointDeviceId,
        string name,
        Windows.Devices.Midi2.Enumeration.MidiEndpointNativeDataFormat format)
    {
        var version = format switch
        {
            Windows.Devices.Midi2.Enumeration.MidiEndpointNativeDataFormat.Midi1ByteFormat => MidiVersion.Midi1,
            Windows.Devices.Midi2.Enumeration.MidiEndpointNativeDataFormat.UniversalMidiPacketFormat => MidiVersion.Midi2,
            _ => MidiVersion.Unknown
        };

        return new MidiInputDevice(
            endpointDeviceId,
            string.IsNullOrWhiteSpace(name) ? "Unnamed MIDI device" : name,
            version);
    }
}
