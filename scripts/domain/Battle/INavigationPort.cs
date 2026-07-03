namespace CasualCastle.Domain.Battle;

public interface INavigationPort
{
	void SetTarget(float gameX, float gameY);
	void ConfigureRvo(float radius, float neighborDistance, float timeHorizon);
}
