using Xunit;

namespace midi_router.Tests;

public sealed class MainWindowThemeTests
{
    [Fact]
    public void MainWindowDeclaresAccessibleGearAndAllAppearanceChoices()
    {
        var path = FindRepositoryFile("MainWindow.xaml");
        var xaml = File.ReadAllText(path);

        Assert.Contains("AutomationProperties.Name=\"Settings\"", xaml);
        Assert.Contains("Content=\"⚙\"", xaml);
        Assert.Contains("Header=\"Light\"", xaml);
        Assert.Contains("Header=\"Dark\"", xaml);
        Assert.Contains("Header=\"OS default\"", xaml);
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
