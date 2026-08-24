namespace midi_router;

public interface ISettingsStore
{
    ApplicationSettings Load();

    void Save(ApplicationSettings settings);
}
