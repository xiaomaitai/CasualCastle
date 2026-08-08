// 单位事件端口：受击、死亡与攻击通知
namespace CasualCastle.Domain.Battle;

public interface IUnitEventPort
{
	void OnDamaged(int amount, IUnitHandle attacker);
	void OnDied();
	void OnAttack(IUnitHandle target, float targetGameX, float targetGameY);
	void OnHeal(IUnitHandle target, int amount);
}
