// 建筑配置仓库端口
using CasualCastle.Domain.Shared;
using System.Collections.Generic;

namespace CasualCastle.Domain.Building;

public interface IBuildingRepository
{
	BuildingData Get(string typeId);

	IReadOnlyList<GridCellOffset> GetFootprint(string typeId) => Get(typeId).Footprint;
	string GetDisplayName(string typeId) => Get(typeId).DisplayName;
	int GetMaxHealth(string typeId) => Get(typeId).MaxHealth;
	float GetSpawnInterval(string typeId) => Get(typeId).SpawnInterval;
	float GetProductionRate(string typeId) => Get(typeId).ProductionRate;
	GridCellOffset GetMainCellOffset(string typeId) => Get(typeId).MainCellOffset;
	GridCellOffset GetSpawnCellOffset(string typeId) => Get(typeId).SpawnCellOffset;
	string GetUnitTypeId(string typeId) => Get(typeId).UnitTypeId;

	int GetCombineTier(string typeId) => Get(typeId).CombineTier;
	bool IsCoreBuilding(string typeId) => Get(typeId).IsCore;
	bool GetHasNightCombat(string typeId) => Get(typeId).HasNightCombat;
	int GetCollisionWidth(string typeId) => Get(typeId).CollisionWidth;
	int GetCollisionHeight(string typeId) => Get(typeId).CollisionHeight;
	int GetValue(string typeId) => Get(typeId).Value;
	string GetFactionId(string typeId) => Get(typeId).FactionId;
	string GetRaceId(string typeId) => Get(typeId).RaceId;
}
