namespace CasualCastle.Domain.Battle;

public interface IRvoService
{
	void ConfigureRvo(INavigationPort navPort, float collisionRadius);
}
