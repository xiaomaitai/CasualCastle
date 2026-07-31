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

	public int LastDamageAmount { get; private set; }
	public IUnitHandle LastAttacker { get; private set; }
	public IUnitHandle LastAttackTarget { get; private set; }
	public float LastAttackTargetX { get; private set; }
	public float LastAttackTargetY { get; private set; }

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
}
