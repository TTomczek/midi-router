using Xunit;

namespace midi_router.Tests;

public sealed class MainWindowTrayTests
{
    [Fact]
    public void MinimizeIsGatedByThePersistedPreference()
    {
        var source = File.ReadAllText(FindRepositoryFile("MainWindow.xaml.cs"));

        Assert.Contains("MinimizeToTray", source);
        Assert.Contains("WindowState == WindowState.Minimized &&", source);
    }

    [Fact]
    public void TrayRestorationUsesASingleLeftClick()
    {
        var source = File.ReadAllText(FindRepositoryFile("MainWindow.xaml.cs"));

        Assert.Contains("MouseClick", source);
        Assert.Contains("MouseButtons.Left", source);
        Assert.DoesNotContain("trayIcon.DoubleClick", source);
    }

    [Fact]
    public void TrayContextMenuProvidesStopActionAndCleanup()
    {
        var source = File.ReadAllText(FindRepositoryFile("MainWindow.xaml.cs"));

        Assert.Contains("Items.Add(\"Stop\"", source);
        Assert.Contains("trayIcon.Visible = false", source);
        Assert.Contains("trayIcon.Dispose()", source);
        Assert.Contains("trayMenu.Dispose()", source);
    }

    private static string FindRepositoryFile(string fileName)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, fileName)))
        {
            directory = directory.Parent;
        }

        return directory is null
            ? throw new FileNotFoundException(fileName)
            : Path.Combine(directory.FullName, fileName);
    }
}
