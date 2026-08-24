using System.Windows;
using Forms = System.Windows.Forms;

namespace midi_router;

public partial class MainWindow : Window
{
    private readonly MidiInputDeviceViewModel viewModel;
    private readonly Forms.NotifyIcon trayIcon;
    private readonly Forms.ContextMenuStrip trayMenu;

    public MainWindow()
    {
        InitializeComponent();
        trayMenu = new Forms.ContextMenuStrip();
        trayMenu.Items.Add("Show", null, (_, _) => RestoreFromTray());
        trayMenu.Items.Add(new Forms.ToolStripSeparator());
        trayMenu.Items.Add("Exit", null, (_, _) => Close());
        trayIcon = new Forms.NotifyIcon
        {
            Icon = System.Drawing.SystemIcons.Application,
            Text = "MIDI Router",
            ContextMenuStrip = trayMenu,
            Visible = true
        };
        trayIcon.DoubleClick += (_, _) => RestoreFromTray();
        viewModel = new MidiInputDeviceViewModel(new WindowsMidiInputDeviceProvider());
        DataContext = viewModel;
        Loaded += async (_, _) => await viewModel.RefreshAsync();
    }

    protected override void OnStateChanged(EventArgs e)
    {
        base.OnStateChanged(e);
        if (WindowState == WindowState.Minimized)
        {
            Hide();
        }
    }

    private void RestoreFromTray()
    {
        Show();
        WindowState = WindowState.Normal;
        Activate();
    }

    protected override void OnClosed(EventArgs e)
    {
        viewModel.Dispose();
        trayIcon.Visible = false;
        trayIcon.Dispose();
        trayMenu.Dispose();
        base.OnClosed(e);
    }
}
