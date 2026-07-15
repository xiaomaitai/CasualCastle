using CasualCastle.Domain.Shared;

namespace CasualCastle.Adapters.Godot.Flow;

public class GameSessionService : IGameSessionService
{
    private readonly ISaveRepository _saveRepo;

    public GameSessionService(ISaveRepository saveRepo)
    {
        _saveRepo = saveRepo;
    }

    public void SaveGame(SaveData data)
    {
        _saveRepo.Save(data);
    }

    public SaveData LoadSaveData(int slot)
    {
        return _saveRepo.Load(slot);
    }

    public bool HasSave(int slot)
    {
        return _saveRepo.HasSave(slot);
    }

    public void DeleteSave(int slot)
    {
        _saveRepo.DeleteSave(slot);
    }
}
