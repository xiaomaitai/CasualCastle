using CasualCastle.Adapters.Godot;
using Godot;

public abstract class AttackBehavior
{
	protected Soldier Soldier { get; }

	protected AttackBehavior(Soldier soldier)
	{
		Soldier = soldier;
	}

	public abstract bool TryExecute(Soldier target, float delta);

	protected bool TargetInRange(Soldier target)
	{
		float dist = Soldier.GlobalPosition.DistanceTo(target.GlobalPosition);
		float myRadius = GameCoordinatesAdapter.GameUnitsToPixels(Soldier.CollisionRadius);
		float targetRadius = GameCoordinatesAdapter.GameUnitsToPixels(target.CollisionRadius);
		return (dist - myRadius - targetRadius) <= Soldier.AttackRange;
	}
}
