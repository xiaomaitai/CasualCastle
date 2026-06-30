using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Battle;
using Godot;

public class GodotPathAccessor : IPathAccessor
{
	private readonly NavigationAgent2D _agent;

	public GodotPathAccessor(NavigationAgent2D agent)
	{
		_agent = agent;
	}

	public bool HasPath => !_agent.IsNavigationFinished();

	public float NextGameX => GameCoordinatesAdapter.PixelsToGameUnits(_agent.GetNextPathPosition().X);

	public float NextGameY => GameCoordinatesAdapter.PixelsToGameUnits(_agent.GetNextPathPosition().Y);

	public void SetTarget(float gameX, float gameY)
	{
		_agent.TargetPosition = new Vector2(
			GameCoordinatesAdapter.GameUnitsToPixels(gameX),
			GameCoordinatesAdapter.GameUnitsToPixels(gameY));
	}
}
