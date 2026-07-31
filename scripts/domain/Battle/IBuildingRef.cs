namespace CasualCastle.Domain.Battle;

public interface IBuildingRef
{
	bool IsDestroyed { get; }
	bool IsCore { get; }
	bool IsEnemyOf(IUnitState unit);
	float MinX { get; }
	float MinY { get; }
	float MaxX { get; }
	float MaxY { get; }
	IBuildingTarget BuildingTarget { get; }
}
