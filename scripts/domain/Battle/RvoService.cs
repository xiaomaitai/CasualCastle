namespace CasualCastle.Domain.Battle;

public class RvoService : IRvoService
{
	private const float NeighborDistanceFactor = 8f;
	private const float TimeHorizon = 0.5f;

	public void ConfigureRvo(INavigationPort navPort, float collisionRadius)
	{
		navPort.ConfigureRvo(collisionRadius, collisionRadius * NeighborDistanceFactor, TimeHorizon);
	}
}
