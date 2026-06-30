namespace CasualCastle.Domain.Battle;

public interface INavigationPort
{
	void SetTarget(float gameX, float gameY);
	(float gameX, float gameY) GetNextPosition(float currentGameX, float currentGameY);
}
