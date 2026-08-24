using System.Text;
using Xunit;

namespace midi_router.Tests;

public sealed class JsonSettingsStoreTests
{
    [Theory]
    [InlineData(AppearanceMode.Light)]
    [InlineData(AppearanceMode.Dark)]
    [InlineData(AppearanceMode.OsDefault)]
    public void RoundTripsAppearanceMode(AppearanceMode mode)
    {
        var path = TemporaryPath();
        try
        {
            var store = new JsonSettingsStore(path);
            store.Save(new ApplicationSettings(mode));
            Assert.Equal(mode, store.Load().AppearanceMode);
        }
        finally
        {
            Delete(path);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RoundTripsMinimizeToTray(bool minimizeToTray)
    {
        var path = TemporaryPath();
        try
        {
            var store = new JsonSettingsStore(path);
            store.Save(new ApplicationSettings(AppearanceMode.Dark, minimizeToTray));

            var settings = store.Load();

            Assert.Equal(AppearanceMode.Dark, settings.AppearanceMode);
            Assert.Equal(minimizeToTray, settings.MinimizeToTray);
        }
        finally
        {
            Delete(path);
        }
    }

    [Fact]
    public void MissingMinimizeToTrayUsesDisabledDefault()
    {
        var path = TemporaryPath();
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, """{"AppearanceMode":"Dark"}""");

            var settings = new JsonSettingsStore(path).Load();

            Assert.False(settings.MinimizeToTray);
        }
        finally
        {
            Delete(path);
        }
    }

    [Fact]
    public void MissingFileUsesOsDefault()
    {
        var path = TemporaryPath();
        Assert.Equal(AppearanceMode.OsDefault, new JsonSettingsStore(path).Load().AppearanceMode);
    }

    [Fact]
    public void MalformedFileThrowsForDiagnosticHandling()
    {
        var path = TemporaryPath();
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, "{", Encoding.UTF8);
            Assert.ThrowsAny<Exception>(() => new JsonSettingsStore(path).Load());
        }
        finally
        {
            Delete(path);
        }
    }

    private static string TemporaryPath()
        => Path.Combine(Path.GetTempPath(), $"midi-router-{Guid.NewGuid():N}", "settings.json");

    private static void Delete(string path)
    {
        var directory = Path.GetDirectoryName(path);
        if (directory is not null && Directory.Exists(directory))
        {
            Directory.Delete(directory, recursive: true);
        }
    }
}
