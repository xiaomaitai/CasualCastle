namespace CasualCastle.Domain.Battle;

public interface IGameState
{
	bool IsPlaying { get; }
	bool IsCombat { get; }
	bool IsPaused { get; }
	int CurrentNightIndex { get; }
}
