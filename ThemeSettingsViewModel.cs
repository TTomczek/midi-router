using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace midi_router;

public sealed class ThemeSettingsViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly ThemeManager themeManager;

    public ThemeSettingsViewModel(ThemeManager themeManager)
    {
        this.themeManager = themeManager;
        Options = new ReadOnlyObservableCollection<AppearanceMode>(
            new ObservableCollection<AppearanceMode>
            {
                AppearanceMode.Light,
                AppearanceMode.Dark,
                AppearanceMode.OsDefault
            });
        SelectModeCommand = new SelectAppearanceModeCommand(this);
        themeManager.Changed += OnThemeChanged;
    }

    public ReadOnlyObservableCollection<AppearanceMode> Options { get; }

    public ICommand SelectModeCommand { get; }

    public AppearanceMode CurrentMode => themeManager.CurrentMode;

    public event PropertyChangedEventHandler? PropertyChanged;

    public void Select(AppearanceMode mode) => themeManager.Select(mode);

    public void Dispose() => themeManager.Changed -= OnThemeChanged;

    private void OnThemeChanged(object? sender, EventArgs e)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentMode)));

    private sealed class SelectAppearanceModeCommand(ThemeSettingsViewModel owner) : ICommand
    {
        event EventHandler? ICommand.CanExecuteChanged
        {
            add { }
            remove { }
        }

        public bool CanExecute(object? parameter) => parameter is AppearanceMode;

        public void Execute(object? parameter)
        {
            if (parameter is AppearanceMode mode)
            {
                owner.Select(mode);
            }
        }
    }
}
