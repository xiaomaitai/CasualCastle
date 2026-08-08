// 游戏状态端口：对局阶段查询
namespace CasualCastle.Domain.Battle;

public interface IGameState
{
	bool IsPlaying { get; }
	bool IsCombat { get; }
	bool IsPaused { get; }
	int CurrentNightIndex { get; }
}
