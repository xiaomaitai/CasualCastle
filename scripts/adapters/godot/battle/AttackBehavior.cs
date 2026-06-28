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
		return dist <= Soldier.AttackRange;
	}
}
