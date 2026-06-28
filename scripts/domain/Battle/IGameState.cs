namespace CasualCastle.Domain.Battle;

public interface IGameState
{
	bool IsPlaying { get; }
	bool IsDay { get; }
	bool IsNight { get; }
	bool IsPaused { get; }
	int CurrentNightIndex { get; }
}
