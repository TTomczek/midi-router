namespace midi_router;

public interface IProfileStore
{
    IEnumerable<string> ListProfileIds();
    Profile Load(string profileId);
    void Save(Profile profile);
    void Delete(string profileId);
}
