using Xunit;

namespace midi_router.Tests;

public sealed class ProfileTests
{
    [Fact]
    public void NormalizationTrimsNamesAndRemovesInvalidState()
    {
        var profile = new Profile("profile-1", "  Alice  ",
            new[] { "device-a", "device-a", " " },
            new Dictionary<string, int>
            {
                ["device-a"] = 2,
                ["device-b"] = 99,
                [" "] = 1
            }).Normalize();

        Assert.Equal("Alice", profile.Name);
        Assert.Equal(new[] { "device-a" }, profile.SelectedDeviceIds);
        Assert.Equal(new Dictionary<string, int> { ["device-a"] = 2 },
            profile.DeviceChannelAssignments);
    }

    [Fact]
    public void BlankNamesAreRejected()
        => Assert.Throws<ArgumentException>(() => new Profile("profile-1", "  "));
}
