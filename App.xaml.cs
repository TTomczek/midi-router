using System.IO;
using System.Diagnostics;
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
        private ILogger<App>? logger;

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
            logger = loggerFactory.CreateLogger<App>();
            var settingsCoordinator = new ApplicationSettingsCoordinator(
                new JsonSettingsStore(),
                message => System.Diagnostics.Debug.WriteLine(message),
                loggerFactory.CreateLogger<ApplicationSettingsCoordinator>());
            settingsCoordinator.Load();
            var profileManager = new ProfileManager(
                new JsonProfileStore(),
                settingsCoordinator,
                message => System.Diagnostics.Debug.WriteLine(message),
                loggerFactory.CreateLogger<ProfileManager>());
            profileManager.Load();
            themeManager = new ThemeManager(
                settingsCoordinator,
                new WindowsOperatingSystemThemeProvider(),
                ApplyPalette);
            themeManager.Load();
            themeSettings = new ThemeSettingsViewModel(themeManager);

            var window = new MainWindow(
                themeSettings, settingsCoordinator, loggerFactory, profileManager);
            MainWindow = window;
            window.Show();
            logger.StartupCompleted();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            var shutdownLogger = logger;
            shutdownLogger?.ShutdownStarted(e.ApplicationExitCode, Environment.CurrentManagedThreadId);

            ExecuteShutdownStep(shutdownLogger, nameof(themeSettings), () => themeSettings?.Dispose());
            ExecuteShutdownStep(shutdownLogger, nameof(themeManager), () => themeManager?.Dispose());
            shutdownLogger?.ShutdownStepStarted(nameof(loggerFactory));
            loggerFactory?.Dispose();
            base.OnExit(e);
        }

        private static void ExecuteShutdownStep(ILogger? shutdownLogger, string step, Action action)
        {
            if (shutdownLogger is null)
            {
                action();
                return;
            }

            var stopwatch = Stopwatch.StartNew();
            shutdownLogger.ShutdownStepStarted(step);
            try
            {
                action();
                shutdownLogger.ShutdownStepCompleted(step, stopwatch.ElapsedMilliseconds);
            }
            catch (Exception exception)
            {
                shutdownLogger.ShutdownStepFailed(exception, step, stopwatch.ElapsedMilliseconds);
                throw;
            }
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
