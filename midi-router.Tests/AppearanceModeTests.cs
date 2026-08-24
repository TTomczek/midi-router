using Xunit;

namespace midi_router.Tests;

public sealed class AppearanceModeTests
{
    [Theory]
    [InlineData("Light", AppearanceMode.Light)]
    [InlineData("dark", AppearanceMode.Dark)]
    [InlineData("OsDefault", AppearanceMode.OsDefault)]
    public void ParseAcceptsSupportedValues(string value, AppearanceMode expected)
        => Assert.Equal(expected, AppearanceModeExtensions.Parse(value));

    [Fact]
    public void ParseFallsBackForUnknownValue()
        => Assert.Equal(AppearanceMode.OsDefault, AppearanceModeExtensions.Parse("unknown"));
}
