using Xunit;

namespace midi_router.Tests;

public sealed class JsonProfileStoreTests
{
    [Fact]
    public void SavesEachProfileInItsOwnFileAndRoundTrips()
    {
        var directory = TestDirectory();
        try
        {
            var store = new JsonProfileStore(directory);
            var first = new Profile("first", "Alice",
                new[] { "device-a" }, new Dictionary<string, int> { ["device-a"] = 3 });
            var second = new Profile("second", "Bob");
            store.Save(first);
            store.Save(second);

            Assert.Equal(new[] { "first", "second" }, store.ListProfileIds());
            var loaded = store.Load("first");
            Assert.Equal(first.Id, loaded.Id);
            Assert.Equal(first.Name, loaded.Name);
            Assert.Equal(first.SelectedDeviceIds, loaded.SelectedDeviceIds);
            Assert.Equal(first.DeviceChannelAssignments, loaded.DeviceChannelAssignments);
            Assert.Equal(first.LastEdited, loaded.LastEdited);
            Assert.True(File.Exists(Path.Combine(directory, "second.json")));
        }
        finally
        {
            Delete(directory);
        }
    }

    private static string TestDirectory()
        => Path.Combine(AppContext.BaseDirectory, "profile-store-tests", Guid.NewGuid().ToString("N"));

    private static void Delete(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
        }
    }
}
