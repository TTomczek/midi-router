using System.Windows;
using Controls = System.Windows.Controls;

namespace midi_router;

internal sealed class ProfilePromptWindow : Window
{
    private readonly Controls.TextBox nameBox;

    private ProfilePromptWindow(string title, string initial)
    {
        Title = title;
        Width = 360;
        Height = 150;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        ResizeMode = ResizeMode.NoResize;
        var panel = new Controls.StackPanel { Margin = new Thickness(16) };
        panel.Children.Add(new Controls.TextBlock { Text = "Profile name:" });
        nameBox = new Controls.TextBox { Text = initial, Margin = new Thickness(0, 8, 0, 12) };
        panel.Children.Add(nameBox);
        var buttons = new Controls.StackPanel { Orientation = Controls.Orientation.Horizontal,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Right };
        var cancel = new Controls.Button { Content = "Cancel", Width = 80, IsCancel = true };
        var ok = new Controls.Button { Content = "OK", Width = 80, IsDefault = true, Margin = new Thickness(8, 0, 0, 0) };
        ok.Click += (_, _) => { if (!string.IsNullOrWhiteSpace(nameBox.Text)) DialogResult = true; };
        buttons.Children.Add(cancel);
        buttons.Children.Add(ok);
        panel.Children.Add(buttons);
        Content = panel;
    }

    public static string? Show(Window owner, string title, string initial)
    {
        var window = new ProfilePromptWindow(title, initial) { Owner = owner };
        return window.ShowDialog() == true ? window.nameBox.Text.Trim() : null;
    }
}
