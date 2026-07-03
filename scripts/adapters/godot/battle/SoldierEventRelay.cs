using CasualCastle.Domain.Battle;
using Godot;

public partial class SoldierEventRelay : Node, ISoldierEventPort
{
	[Signal]
	public delegate void DamagedEventHandler();

	[Signal]
	public delegate void DiedEventHandler();

	public int LastDamageAmount { get; private set; }
	public ISoldierHandle LastAttacker { get; private set; }

	public void OnDamaged(int amount, ISoldierHandle attacker)
	{
		LastDamageAmount = amount;
		LastAttacker = attacker;
		EmitSignal(SignalName.Damaged);
	}

	public void OnDied()
	{
		EmitSignal(SignalName.Died);
	}
}
