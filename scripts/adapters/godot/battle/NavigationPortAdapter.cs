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

	public (float gameX, float gameY) GetNextPosition(float currentGameX, float currentGameY)
	{
		Vector2 next = _agent.GetNextPathPosition();
		return (
			GameCoordinatesAdapter.PixelsToGameUnits(next.X),
			GameCoordinatesAdapter.PixelsToGameUnits(next.Y));
	}
}
