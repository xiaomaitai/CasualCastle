namespace CasualCastle.Domain.Battle;

public interface ISoldierEventPort
{
	void OnDamaged(int amount, ISoldierHandle attacker);
	void OnDied();
}
