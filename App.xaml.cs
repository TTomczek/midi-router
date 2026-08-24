using System.Windows;

namespace midi_router
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private ThemeManager? themeManager;
        private ThemeSettingsViewModel? themeSettings;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var settingsCoordinator = new ApplicationSettingsCoordinator(
                new JsonSettingsStore(),
                message => System.Diagnostics.Debug.WriteLine(message));
            themeManager = new ThemeManager(
                settingsCoordinator,
                new WindowsOperatingSystemThemeProvider(),
                ApplyPalette);
            themeManager.Load();
            themeSettings = new ThemeSettingsViewModel(themeManager);

            var window = new MainWindow(themeSettings, settingsCoordinator);
            MainWindow = window;
            window.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            themeSettings?.Dispose();
            themeManager?.Dispose();
            base.OnExit(e);
        }

        private void ApplyPalette(ThemePalette palette)
        {
            var uri = new Uri(
                palette == ThemePalette.Dark
                    ? "ThemeResources/Dark.xaml"
                    : "ThemeResources/Light.xaml",
                UriKind.Relative);
            var dictionary = new ResourceDictionary { Source = uri };
            Resources.MergedDictionaries.Clear();
            Resources.MergedDictionaries.Add(dictionary);
        }
    }

}
