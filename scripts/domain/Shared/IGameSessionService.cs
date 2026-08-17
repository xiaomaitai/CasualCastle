// 对局会话服务端口：保存、读取与恢复
namespace CasualCastle.Domain.Shared;

public interface IGameSessionService
{
	void SaveGame(int slot);
	SaveData LoadSaveData(int slot);
	bool HasSave(int slot);
	void DeleteSave(int slot);
	void RestoreSession(SaveData data);
}
