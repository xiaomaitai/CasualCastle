namespace CasualCastle.Domain.Battle;

public interface IUnitHandle : IUnitState
{
	void TakeDamage(int amount, IUnitHandle attacker, float attackerGameX, float attackerGameY, bool isAoe = false);
	int Heal(int amount);
	void SetEnemyTarget(IUnitHandle target);
	void SetStealth(bool stealthed);
	void ApplyPush(float dx, float dy);
	void ApplySlow(float slowValue, float duration);
	void SetBuildingTarget(IBuildingTarget building);
	void ClearBuildingTarget();
	void SetGameContext(GameContext ctx);
}
