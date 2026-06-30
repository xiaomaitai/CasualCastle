namespace CasualCastle.Domain.Battle;

public interface IPathAccessor
{
	bool HasPath { get; }
	float NextGameX { get; }
	float NextGameY { get; }
	void SetTarget(float gameX, float gameY);
}
