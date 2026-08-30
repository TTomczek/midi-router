using System.Windows;
using System.Windows.Input;
using Controls = System.Windows.Controls;

namespace midi_router;

public sealed class ProfileNameDialog : Window
{
    private readonly Controls.TextBox nameInput;

    public ProfileNameDialog(string title, string initialName = "")
    {
        Title = title;
        Width = 360;
        MinWidth = 360;
        SizeToContent = SizeToContent.Height;
        ResizeMode = ResizeMode.NoResize;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = (System.Windows.Media.Brush)FindResource("WindowBackgroundBrush");
        Foreground = (System.Windows.Media.Brush)FindResource("PrimaryTextBrush");
        ShowInTaskbar = false;

        var layout = new Controls.Grid { Margin = new Thickness(24) };
        layout.RowDefinitions.Add(new Controls.RowDefinition { Height = GridLength.Auto });
        layout.RowDefinitions.Add(new Controls.RowDefinition { Height = GridLength.Auto });
        layout.RowDefinitions.Add(new Controls.RowDefinition { Height = GridLength.Auto });

        layout.Children.Add(new Controls.TextBlock
        {
            Text = "Profile name",
            FontSize = 12,
            Foreground = (System.Windows.Media.Brush)FindResource("SecondaryTextBrush")
        });

        nameInput = new Controls.TextBox
        {
            Text = initialName,
            Margin = new Thickness(0, 8, 0, 16),
            Height = 30
        };
        System.Windows.Automation.AutomationProperties.SetName(nameInput, "Profile name");
        nameInput.KeyDown += NameInput_KeyDown;
        Controls.Grid.SetRow(nameInput, 1);
        layout.Children.Add(nameInput);

        var buttons = new Controls.StackPanel
        {
            Orientation = Controls.Orientation.Horizontal,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Right
        };
        var cancel = new Controls.Button { Content = "Cancel", IsCancel = true, Margin = new Thickness(0, 0, 8, 0) };
        cancel.Click += (_, _) => DialogResult = false;
        var accept = new Controls.Button { Content = "OK", IsDefault = true };
        accept.Click += (_, _) => Accept();
        buttons.Children.Add(cancel);
        buttons.Children.Add(accept);
        Controls.Grid.SetRow(buttons, 2);
        layout.Children.Add(buttons);

        Content = layout;
        Loaded += (_, _) =>
        {
            nameInput.Focus();
            nameInput.SelectAll();
        };
    }

    public string ProfileName => nameInput.Text.Trim();

    private void NameInput_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            Accept();
            e.Handled = true;
        }
    }

    private void Accept()
    {
        if (string.IsNullOrWhiteSpace(nameInput.Text))
        {
            System.Windows.MessageBox.Show(
                this,
                "Profile name cannot be empty.",
                "Invalid profile name",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information);
            return;
        }

        DialogResult = true;
    }
}
