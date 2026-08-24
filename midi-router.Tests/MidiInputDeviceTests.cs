using Windows.Devices.Midi2.Enumeration;
using Xunit;

namespace midi_router.Tests;

public sealed class MidiInputDeviceTests
{
    [Fact]
    public void ProjectionUsesEndpointIdentityAndNativeFormat()
    {
        var device = MidiDeviceProjection.FromEndpoint(
            "endpoint-a",
            "Keyboard",
            MidiEndpointNativeDataFormat.Midi1ByteFormat);

        Assert.Equal("endpoint-a", device.EndpointDeviceId);
        Assert.Equal("Keyboard", device.Name);
        Assert.Equal(MidiVersion.Midi1, device.Version);
    }

    [Fact]
    public void UnknownNativeFormatRemainsUnknown()
    {
        var device = MidiDeviceProjection.FromEndpoint(
            "endpoint-a",
            "Device",
            MidiEndpointNativeDataFormat.Unknown);

        Assert.Equal(MidiVersion.Unknown, device.Version);
    }
}
