namespace CasualCastle.Domain.Battle;

public interface ISoldierHandle : ISoldierState
{
	void TakeDamage(int amount, ISoldierHandle attacker, float attackerGameX, float attackerGameY);
	void SetEnemyTarget(ISoldierHandle target);
	void ApplyPush(float dx, float dy);
	void SetBuildingTarget(IBuildingTarget building);
	void ClearBuildingTarget();
}
