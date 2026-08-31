using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using Forms = System.Windows.Forms;
using Controls = System.Windows.Controls;

namespace midi_router;

public partial class MainWindow : Window
{
    private readonly MidiInputDeviceViewModel viewModel;
    private readonly Forms.NotifyIcon trayIcon;
    private readonly Forms.ContextMenuStrip trayMenu;
    private readonly ThemeSettingsViewModel themeSettings;
    private readonly ProfileManager? profileManager;
    private readonly ILoggerFactory? loggerFactory;
    private MidiRouterDeviceCoordinator? routerCoordinator;
    private bool routingInitializationStarted;

    public MainWindow(
        ThemeSettingsViewModel themeSettings,
        ApplicationSettingsCoordinator? settingsCoordinator = null,
        ILoggerFactory? loggerFactory = null,
        ProfileManager? profileManager = null)
    {
        InitializeComponent();
        this.themeSettings = themeSettings;
        this.loggerFactory = loggerFactory;
        this.profileManager = profileManager;
        DataContext = themeSettings;
        trayMenu = new Forms.ContextMenuStrip();
        trayMenu.Items.Add("Show", null, (_, _) => RestoreFromTray());
        trayMenu.Items.Add(new Forms.ToolStripSeparator());
        trayMenu.Items.Add("Stop", null, (_, _) => Close());
        trayIcon = new Forms.NotifyIcon
        {
            Icon = System.Drawing.SystemIcons.Application,
            Text = "MIDI Router",
            ContextMenuStrip = trayMenu,
            Visible = true
        };
        trayIcon.MouseClick += TrayIcon_MouseClick;
        viewModel = new MidiInputDeviceViewModel(
            new WindowsMidiInputDeviceProvider(
                loggerFactory?.CreateLogger<WindowsMidiInputDeviceProvider>()),
            settingsCoordinator,
            loggerFactory,
            profileManager);
        DeviceList.DataContext = viewModel;
        if (profileManager is not null)
        {
            ProfileSelector.DataContext = profileManager;
            ProfileSelector.ItemsSource = profileManager.ProfileItems;
            ProfileSelector.SelectedValuePath = nameof(ProfileListItem.Id);
            ProfileSelector.SelectedValue = profileManager.ActiveProfileId;
            profileManager.Diagnostic += ProfileManager_Diagnostic;
            profileManager.ActiveProfileChanged += ProfileManager_ActiveProfileChanged;
            profileManager.ProfilesChanged += ProfileManager_ProfilesChanged;
        }
        ContentRendered += MainWindow_ContentRendered;
    }

    private async void MainWindow_ContentRendered(object? sender, EventArgs e)
    {
        ContentRendered -= MainWindow_ContentRendered;
        await Task.Run(() => viewModel.RefreshAsync());
        await Task.Run(InitializeRouting);
    }

    private void InitializeRouting()
    {
        if (routingInitializationStarted)
        {
            return;
        }

        routingInitializationStarted = true;
        if (!Windows.Devices.Midi2.MidiApi.EnsureServiceAvailable())
        {
            viewModel.StatusMessageFromRouter("Windows MIDI Services is unavailable.");
            return;
        }

        WindowsMidiRoutingEndpointProvider? routingProvider = null;
        MidiRouter? router = null;
        try
        {
            routingProvider = new WindowsMidiRoutingEndpointProvider(
                logger: loggerFactory?.CreateLogger<WindowsMidiRoutingEndpointProvider>());
            router = new MidiRouter(
                routingProvider,
                logger: loggerFactory?.CreateLogger<MidiRouter>());
            router.Diagnostic += (_, message) => viewModel.StatusMessageFromRouter(message);
            routerCoordinator = new MidiRouterDeviceCoordinator(
                viewModel,
                router,
                loggerFactory?.CreateLogger<MidiRouterDeviceCoordinator>());
        }
        catch (InvalidOperationException exception)
        {
            router?.Dispose();
            routingProvider?.Dispose();
            viewModel.StatusMessageFromRouter(exception.Message);
        }
        catch (System.Runtime.InteropServices.COMException exception)
        {
            router?.Dispose();
            routingProvider?.Dispose();
            viewModel.StatusMessageFromRouter(
                $"MIDI routing could not be started: {exception.Message}");
        }
    }

    protected override void OnStateChanged(EventArgs e)
    {
        base.OnStateChanged(e);
        if (WindowState == WindowState.Minimized && themeSettings.MinimizeToTray)
        {
            Hide();
        }
    }

    private void TrayIcon_MouseClick(object? sender, Forms.MouseEventArgs e)
    {
        if (e.Button == Forms.MouseButtons.Left)
        {
            RestoreFromTray();
        }
    }

    private void RestoreFromTray()
    {
        Show();
        WindowState = WindowState.Normal;
        Activate();
    }

    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Controls.Button button)
        {
            button.ContextMenu.IsOpen = true;
        }
    }

    private void SettingsMenu_Opened(object sender, RoutedEventArgs e)
    {
        LightMenuItem.IsChecked = themeSettings.CurrentMode == AppearanceMode.Light;
        DarkMenuItem.IsChecked = themeSettings.CurrentMode == AppearanceMode.Dark;
        OsDefaultMenuItem.IsChecked = themeSettings.CurrentMode == AppearanceMode.OsDefault;
        MinimizeToTrayMenuItem.IsChecked = themeSettings.MinimizeToTray;
    }

    private void MinimizeToTrayMenuItem_Click(object sender, RoutedEventArgs e)
        => themeSettings.SetMinimizeToTray(MinimizeToTrayMenuItem.IsChecked);

    private void AppearanceMenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Controls.MenuItem { Tag: string value })
        {
            themeSettings.Select(AppearanceModeExtensions.Parse(value));
        }
    }

    private void ProfileSelector_SelectionChanged(object sender, Controls.SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0 || profileManager is null ||
                e.AddedItems[0] is not ProfileListItem item)
            {
                return;
            }
            if (item.IsCreate)
            {
                ProfileSelector.SelectedValue = profileManager.ActiveProfileId;
                ShowProfileNameDialog(null);
                return;
            }
            profileManager.Select(item.Id);
            ProfileSelector.IsDropDownOpen = false;
        }

        private void ProfileEntry_ContextMenuOpening(object sender, Controls.ContextMenuEventArgs e)
        {
            if (sender is FrameworkElement { DataContext: ProfileListItem { IsCreate: true } })
            {
                e.Handled = true;
            }
        }

        private void RenameProfileMenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (profileManager is null || sender is not Controls.MenuItem { Tag: string id })
        {
            return;
        }

        var item = profileManager.ProfileItems.FirstOrDefault(profile => profile.Id == id);
        if (item is not null)
        {
            ShowProfileNameDialog(item);
        }
    }

    private void DeleteProfileMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (profileManager is null || sender is not Controls.MenuItem { Tag: string id })
            {
                return;
            }
            var item = profileManager.ProfileItems.FirstOrDefault(profile => profile.Id == id);
            if (item is null || System.Windows.MessageBox.Show(
                    $"Delete profile '{item.DisplayName}'?",
                    "Delete profile", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
            {
                return;
            }
            profileManager.Delete(id);
        }

    private void ShowProfileNameDialog(ProfileListItem? item)
    {
        if (profileManager is null)
        {
            return;
        }

        var dialog = new ProfileNameDialog(
            item is null ? "Create profile" : "Rename profile",
            item?.Name ?? string.Empty)
        {
            Owner = this
        };
        if (dialog.ShowDialog() != true)
        {
            return;
        }

        var committed = item is null
            ? profileManager.TryCreate(dialog.ProfileName, out _)
            : profileManager.TryRename(item.Id, dialog.ProfileName);
        if (committed)
        {
            ProfileSelector.IsDropDownOpen = false;
        }
    }

    private void ProfileManager_Diagnostic(object? sender, string message)
        => viewModel.StatusMessageFromRouter(message);

    private void ProfileManager_ActiveProfileChanged(object? sender, EventArgs e)
    {
        if (profileManager is not null)
        {
            ProfileSelector.SelectedValue = profileManager.ActiveProfileId;
        }
    }

    private void ProfileManager_ProfilesChanged(object? sender, EventArgs e)
    {
        if (profileManager is not null)
        {
            ProfileSelector.SelectedValue = profileManager.ActiveProfileId;
        }
    }

    private void DeviceList_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource is not DependencyObject source ||
            FindVisualParent<Controls.ComboBox>(source) is not null ||
            Controls.ItemsControl.ContainerFromElement(DeviceList, source) is not Controls.ListViewItem item ||
            item.DataContext is not MidiInputDeviceRow row)
        {
            return;
        }

        viewModel.ToggleSelection(row.EndpointDeviceId);
        e.Handled = true;
    }

    private static T? FindVisualParent<T>(DependencyObject source)
        where T : DependencyObject
    {
        for (var current = source; current is not null; current = System.Windows.Media.VisualTreeHelper.GetParent(current))
        {
            if (current is T match)
            {
                return match;
            }
        }

        return null;
    }

    private void ChannelComboBox_SelectionChanged(object sender, Controls.SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count == 0 ||
            sender is not Controls.ComboBox comboBox ||
            comboBox.DataContext is not MidiInputDeviceRow row ||
            e.AddedItems[0] is not int displayChannel)
        {
            return;
        }

        viewModel.SetChannel(row.EndpointDeviceId, displayChannel);
    }

    protected override void OnClosed(EventArgs e)
    {
        ContentRendered -= MainWindow_ContentRendered;
        viewModel.Dispose();
        if (profileManager is not null)
        {
            profileManager.Diagnostic -= ProfileManager_Diagnostic;
            profileManager.ActiveProfileChanged -= ProfileManager_ActiveProfileChanged;
            profileManager.ProfilesChanged -= ProfileManager_ProfilesChanged;
        }
        routerCoordinator?.Dispose();
        trayIcon.MouseClick -= TrayIcon_MouseClick;
        trayIcon.Visible = false;
        trayIcon.Dispose();
        trayMenu.Dispose();
        base.OnClosed(e);
    }
}
