using System.IO;
using System.Windows;
using Microsoft.Extensions.Logging;

namespace midi_router
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private ThemeManager? themeManager;
        private ThemeSettingsViewModel? themeSettings;
        private ILoggerFactory? loggerFactory;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var logDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MIDI Router",
                "Logs");
            loggerFactory = LoggerFactory.Create(builder =>
                builder
                    .SetMinimumLevel(LogLevel.Trace)
                    .AddProvider(new RotatingFileLoggerProvider(
                        Path.Combine(logDirectory, "midi-router.log"))));
            var settingsCoordinator = new ApplicationSettingsCoordinator(
                new JsonSettingsStore(),
                message => System.Diagnostics.Debug.WriteLine(message));
            settingsCoordinator.Load();
            themeManager = new ThemeManager(
                settingsCoordinator,
                new WindowsOperatingSystemThemeProvider(),
                ApplyPalette);
            themeManager.Load();
            themeSettings = new ThemeSettingsViewModel(themeManager);

            var window = new MainWindow(themeSettings, settingsCoordinator, loggerFactory);
            MainWindow = window;
            window.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            themeSettings?.Dispose();
            themeManager?.Dispose();
            loggerFactory?.Dispose();
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
