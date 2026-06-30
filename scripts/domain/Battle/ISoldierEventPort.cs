namespace CasualCastle.Domain.Battle;

public interface ISoldierEventPort
{
	void OnDamaged(int amount, ISoldierService attacker);
	void OnDied();
}
