// 建筑引用端口：供战斗查询建筑边界与阵营
namespace CasualCastle.Domain.Battle;

public interface IBuildingRef
{
	string TypeId { get; }
	bool IsDestroyed { get; }
	bool IsCore { get; }
	bool IsEnemyOf(IUnitState unit);
	float MinX { get; }
	float MinY { get; }
	float MaxX { get; }
	float MaxY { get; }
	IBuildingTarget BuildingTarget { get; }
}
