// 单位事件端口：受击、死亡、攻击、治疗与召唤通知
namespace CasualCastle.Domain.Battle;

public interface IUnitEventPort
{
	void OnDamaged(int amount, IUnitHandle attacker, bool isDot, bool isHeavy);
	void OnDied();
	void OnAttack(IUnitHandle target, float targetGameX, float targetGameY);
	void OnHeal(IUnitHandle target, int amount);
	void OnSummon(string unitTypeId, float x, float y);
}