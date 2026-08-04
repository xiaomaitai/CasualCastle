using CasualCastle.Domain.Battle;
using Godot;

public partial class UnitEventRelay : Node, IUnitEventPort
{
	[Signal]
	public delegate void DamagedEventHandler();

	[Signal]
	public delegate void DiedEventHandler();

	[Signal]
	public delegate void AttackEventHandler();

	[Signal]
	public delegate void HealedEventHandler();

	public int LastDamageAmount { get; private set; }
	public IUnitHandle LastAttacker { get; private set; }
	public IUnitHandle LastAttackTarget { get; private set; }
	public float LastAttackTargetX { get; private set; }
	public float LastAttackTargetY { get; private set; }
	public IUnitHandle LastHealTarget { get; private set; }
	public int LastHealAmount { get; private set; }
	public float LastHealTargetX { get; private set; }
	public float LastHealTargetY { get; private set; }

	public void OnDamaged(int amount, IUnitHandle attacker)
	{
		LastDamageAmount = amount;
		LastAttacker = attacker;
		EmitSignal(SignalName.Damaged);
	}

	public void OnDied()
	{
		EmitSignal(SignalName.Died);
	}

	public void OnAttack(IUnitHandle target, float targetGameX, float targetGameY)
	{
		LastAttackTarget = target;
		LastAttackTargetX = targetGameX;
		LastAttackTargetY = targetGameY;
		EmitSignal(SignalName.Attack);
	}

	public void OnHeal(IUnitHandle target, int amount)
	{
		LastHealTarget = target;
		LastHealAmount = amount;
		LastHealTargetX = target.GameX;
		LastHealTargetY = target.GameY;
		EmitSignal(SignalName.Healed);
	}
}
