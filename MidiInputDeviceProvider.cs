using System.Runtime.InteropServices;
using Windows.Devices.Midi2.Enumeration;

namespace midi_router;

public enum MidiVersion
{
    Midi1,
    Midi2
}

public sealed record MidiInputDevice(
    string Name,
    MidiVersion Version = MidiVersion.Midi1,
    string EndpointDeviceId = "")
{
    public int TargetChannel { get; set; } = 1;
    public string VersionDisplayName => Version == MidiVersion.Midi2 ? "MIDI 2" : "MIDI 1";
}

public interface IMidiInputDeviceProvider
{
    IReadOnlyList<MidiInputDevice> GetDevices();
}

public sealed class WindowsMidiInputDeviceProvider : IMidiInputDeviceProvider
{
    public IReadOnlyList<MidiInputDevice> GetDevices()
    {
        try
        {
            var devices = new List<MidiInputDevice>();

            AddDevices(devices, MidiEndpointDeviceInformationFilters.StandardNativeMidi1ByteFormat, MidiVersion.Midi1);
            AddDevices(devices, MidiEndpointDeviceInformationFilters.StandardNativeUniversalMidiPacketFormat, MidiVersion.Midi2);

            return devices;
        }
        catch (Exception exception) when (exception is COMException or InvalidOperationException or UnauthorizedAccessException)
        {
            throw new InvalidOperationException("MIDI-Eingabegeräte konnten über Windows MIDI Services nicht gelesen werden.", exception);
        }
    }

    private static void AddDevices(
        ICollection<MidiInputDevice> devices,
        MidiEndpointDeviceInformationFilters filter,
        MidiVersion version)
    {
        var deviceInfos = MidiEndpointDeviceInformation.FindAll(
            MidiEndpointDeviceInformationSortOrder.Name,
            filter);

        foreach (var deviceInfo in deviceInfos)
        {
            var name = string.IsNullOrWhiteSpace(deviceInfo.Name)
                ? deviceInfo.EndpointDeviceId
                : deviceInfo.Name.Trim();
            devices.Add(new MidiInputDevice(name, version, deviceInfo.EndpointDeviceId));
        }
    }
}
