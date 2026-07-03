using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Battle;
using Godot;

public class NavigationPortAdapter : INavigationPort
{
	private NavigationAgent2D _agent;

	public NavigationPortAdapter(NavigationAgent2D agent)
	{
		_agent = agent;
	}

	public void SetTarget(float gameX, float gameY)
	{
		_agent.TargetPosition = new Vector2(
			GameCoordinatesAdapter.GameUnitsToPixels(gameX),
			GameCoordinatesAdapter.GameUnitsToPixels(gameY));
	}

	public void ConfigureRvo(float radius, float neighborDistance, float timeHorizon)
	{
		_agent.Radius = GameCoordinatesAdapter.GameUnitsToPixels(radius);
		_agent.NeighborDistance = GameCoordinatesAdapter.GameUnitsToPixels(neighborDistance);
		_agent.TimeHorizonAgents = timeHorizon;
	}
}
