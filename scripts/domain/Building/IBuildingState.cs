// 建筑状态端口：领域内供规则消费
namespace CasualCastle.Domain.Building;

public interface IBuildingState : IAdjacencyBuilding
{
	bool IsDestroyed { get; }
	bool IsManuallyPaused { get; }
	bool IsCombineProhibited { get; }
	bool HasEnemyOnTop { get; }
	bool IsPlayerOwned { get; }
}
