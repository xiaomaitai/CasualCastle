// 城堡建筑管理：核心之心、兵营与初始布局
using CasualCastle.Domain.Shared;
using CasualCastle.Domain.Building;
using CasualCastle.Adapters.Godot;
using Godot;
using System.Collections.Generic;

public partial class Castle
{
	private void SetupCastleHeart()
	{
		string heartTypeId = GameManager.Instance != null && GameManager.Instance.SelectedFaction == FactionDef.HordeId
			? HordeCoreRules.CoreLevelTypeIds[0]
			: GameManager.Instance != null && GameManager.Instance.SelectedFaction == FactionDef.NecromancerId
				? RitualCircleRules.CoreLevelTypeIds[0]
				: BuildingTypeIds.CastleHeart;
		Building heart = _buildingSystem.CreateBuilding(heartTypeId);
		if (heart == null)
			return;

		heart.BindToGrid(this, CastleHeartGridX, CastleHeartGridY);
		PlaceBuilding(heart, CastleHeartGridX, CastleHeartGridY, heartTypeId);
		Heart = heart;
		heart.HealthChanged += OnHeartHealthChanged;
	}

	public void ReplaceHeart(string typeId, int health)
	{
		if (Heart == null)
			return;

		Building oldHeart = Heart;
		ReleaseBuildingFootprint(oldHeart);
		oldHeart.GetParent()?.RemoveChild(oldHeart);
		oldHeart.QueueFree();

		Building heart = _buildingSystem.CreateBuilding(typeId);
		if (heart == null)
			return;

		heart.BindToGrid(this, CastleHeartGridX, CastleHeartGridY);
		PlaceBuilding(heart, CastleHeartGridX, CastleHeartGridY, typeId);
		heart.ApplySnapshotState(health, false, false);
		Heart = heart;
		heart.HealthChanged += OnHeartHealthChanged;
	}

	private void SetupBarracks()
	{
		if (!IsPlayerCastle)
			return;

		PlaceInitialBarracks(BarracksGridX, BarracksGridY);
		if (SecondBarracksGridX >= 0 && SecondBarracksGridY >= 0)
			PlaceInitialBarracks(SecondBarracksGridX, SecondBarracksGridY);
	}

	private void PlaceInitialBarracks(int gridX, int gridY)
	{
		Building building = _buildingSystem.CreateBuilding(BuildingTypeIds.Barracks);
		if (building == null)
			return;

		building.BindToGrid(this, gridX, gridY);
		PlaceBuilding(building, gridX, gridY);
	}

}
