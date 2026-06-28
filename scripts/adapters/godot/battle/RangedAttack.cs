using CasualCastle.Domain.Battle;
using Godot;

public class RangedAttack : AttackBehavior
{
	public RangedAttack(Soldier soldier) : base(soldier) { }

	public override bool TryExecute(Soldier target, float delta)
	{
		if (!TargetInRange(target))
			return false;

		Projectile projectile = new Projectile();
		Soldier.GetParent().AddChild(projectile);
		projectile.Launch(Soldier.GlobalPosition, target, Soldier.Damage, Soldier.Data.DamageType);
		return true;
	}
}
