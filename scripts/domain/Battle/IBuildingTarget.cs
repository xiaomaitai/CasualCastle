// 建筑目标端口：可受击建筑
namespace CasualCastle.Domain.Battle;

public interface IBuildingTarget
{
	bool IsDestroyed { get; }
	void TakeDamage(int amount);
}
