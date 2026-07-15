namespace CasualCastle.Domain.Shared;

public interface IGameSessionService
{
    void SaveGame(SaveData data);
    SaveData LoadSaveData(int slot);
    bool HasSave(int slot);
    void DeleteSave(int slot);
}
