using Xunit;

namespace midi_router.Tests;

public sealed class MainWindowLayoutTests
{
    [Fact]
    public void DeviceListUsesBoundedResponsiveLayout()
    {
        var xaml = File.ReadAllText(FindRepositoryFile("MainWindow.xaml"));

        Assert.Contains("ScrollViewer.HorizontalScrollBarVisibility=\"Disabled\"", xaml);
        Assert.Contains("<ListView.ItemTemplate>", xaml);
        Assert.Contains("TextTrimming=\"CharacterEllipsis\"", xaml);
        Assert.Contains("DeviceActivityActiveBrush", xaml);
    }

    [Fact]
    public void ProfileSelectorIsBeforeSettingsAndSupportsEditingAndDeletion()
    {
        var xaml = File.ReadAllText(FindRepositoryFile("MainWindow.xaml"));

        Assert.Contains("x:Name=\"ProfileSelector\"", xaml);
        Assert.Contains("DockPanel.Dock=\"Right\"", xaml);
        Assert.Contains("ProfileNameDialog", File.ReadAllText(FindRepositoryFile("ProfileNameDialog.cs")));
        Assert.Contains("RenameProfileMenuItem_Click", xaml);
        Assert.Contains("DeleteProfileMenuItem_Click", xaml);
        Assert.Contains("Header=\"Rename\"", xaml);
        Assert.Contains("Header=\"Delete\"", xaml);
        Assert.Contains("ContextMenuOpening=\"ProfileEntry_ContextMenuOpening\"", xaml);
        Assert.Contains("HorizontalContentAlignment\" Value=\"Stretch\"", xaml);
        Assert.Contains("Background=\"Transparent\"", xaml);
        Assert.Contains("Binding=\"{Binding IsDeletable}\" Value=\"True\"", xaml);
        Assert.Contains("Binding=\"{Binding CanRename}\" Value=\"True\"", xaml);
        Assert.Contains("AutomationProperties.Name=\"Active profile\"", xaml);
        Assert.DoesNotContain("SelectedValue=\"{Binding ActiveProfileId", xaml);
        Assert.Contains("ProfileManager_ProfilesChanged", File.ReadAllText(FindRepositoryFile("MainWindow.xaml.cs")));
    }

    private static string FindRepositoryFile(string fileName)
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory is not null;
             directory = directory.Parent)
        {
            var candidate = Path.Combine(directory.FullName, fileName);
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        throw new FileNotFoundException($"Could not locate {fileName}.");
    }
}
