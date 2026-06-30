using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Battle;
using Godot;

public class GodotPositionAccessor : IPositionAccessor
{
	private readonly Node2D _node;

	public GodotPositionAccessor(Node2D node)
	{
		_node = node;
	}

	public float GameX
	{
		get => GameCoordinatesAdapter.PixelsToGameUnits(_node.GlobalPosition.X);
		set
		{
			Vector2 pos = _node.GlobalPosition;
			pos.X = GameCoordinatesAdapter.GameUnitsToPixels(value);
			_node.GlobalPosition = pos;
		}
	}

	public float GameY
	{
		get => GameCoordinatesAdapter.PixelsToGameUnits(_node.GlobalPosition.Y);
		set
		{
			Vector2 pos = _node.GlobalPosition;
			pos.Y = GameCoordinatesAdapter.GameUnitsToPixels(value);
			_node.GlobalPosition = pos;
		}
	}
}
