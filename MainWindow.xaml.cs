using System.Windows;
using Forms = System.Windows.Forms;
using Controls = System.Windows.Controls;

namespace midi_router;

public partial class MainWindow : Window
{
    private readonly MidiInputDeviceViewModel viewModel;
    private readonly Forms.NotifyIcon trayIcon;
    private readonly Forms.ContextMenuStrip trayMenu;
    private readonly ThemeSettingsViewModel themeSettings;

    public MainWindow(ThemeSettingsViewModel themeSettings)
    {
        InitializeComponent();
        this.themeSettings = themeSettings;
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
        viewModel = new MidiInputDeviceViewModel(new WindowsMidiInputDeviceProvider());
        DeviceList.DataContext = viewModel;
        Loaded += async (_, _) => await viewModel.RefreshAsync();
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

    protected override void OnClosed(EventArgs e)
    {
        viewModel.Dispose();
        trayIcon.MouseClick -= TrayIcon_MouseClick;
        trayIcon.Visible = false;
        trayIcon.Dispose();
        trayMenu.Dispose();
        base.OnClosed(e);
    }
}
