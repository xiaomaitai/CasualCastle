using System.Collections.Generic;

namespace CasualCastle.Domain.Shared;

public interface ISaveRepository
{
    void Save(SaveData data);
    SaveData Load(int slotIndex);
    bool HasSave(int slotIndex);
    void DeleteSave(int slotIndex);
    List<int> ListSlots();
}
