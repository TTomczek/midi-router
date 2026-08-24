using System.IO;
using Xunit;

namespace midi_router.Tests;

public sealed class ApplicationSettingsCoordinatorTests
{
    [Fact]
    public void UpdatePreservesUnrelatedSettings()
    {
        var store = new Store(new ApplicationSettings(AppearanceMode.Dark, true));
        var coordinator = new ApplicationSettingsCoordinator(store);
        coordinator.Load();

        coordinator.Update(
            current => current with { SelectedDeviceIds = new[] { "device-a" } },
            "Device selection");

        Assert.Equal(AppearanceMode.Dark, store.Settings.AppearanceMode);
        Assert.True(store.Settings.MinimizeToTray);
        Assert.Equal(new[] { "device-a" }, store.Settings.SelectedDeviceIds);
    }

    [Fact]
    public void UpdateReportsPersistenceFailure()
    {
        var messages = new List<string>();
        var coordinator = new ApplicationSettingsCoordinator(new ThrowingStore(), messages.Add);

        coordinator.Update(
            current => current with { SelectedDeviceIds = new[] { "device-a" } },
            "Device selection");

        Assert.Contains(messages, message => message.Contains("could not be saved"));
    }

    private sealed class Store(ApplicationSettings initial) : ISettingsStore
    {
        public ApplicationSettings Settings { get; private set; } = initial;
        public ApplicationSettings Load() => Settings;
        public void Save(ApplicationSettings settings) => Settings = settings;
    }

    private sealed class ThrowingStore : ISettingsStore
    {
        public ApplicationSettings Load() => new();
        public void Save(ApplicationSettings settings) => throw new IOException("write failed");
    }
}
