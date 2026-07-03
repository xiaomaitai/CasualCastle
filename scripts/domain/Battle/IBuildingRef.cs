namespace CasualCastle.Domain.Battle;

public interface IBuildingRef
{
	bool IsDestroyed { get; }
	bool IsEnemyOf(ISoldierState soldier);
	float MinX { get; }
	float MinY { get; }
	float MaxX { get; }
	float MaxY { get; }
	IBuildingTarget BuildingTarget { get; }
	object CastleRef { get; }
}
