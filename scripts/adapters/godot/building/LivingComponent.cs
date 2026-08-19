// 活体建筑组件：再生、生长、繁殖与自动成长
using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Battle;
using CasualCastle.Domain.Building;
using System;

public sealed class LivingComponent : IBuildingComponent
{
	public event Action Upgraded;

	public event Action<int> HealthRestored;

	private readonly Building _owner;

	private float _growthAccumulator;

	private float _regenAccumulator;

	private float _breedAccumulator;

	public LivingComponent(Building owner)
	{
		_owner = owner;
	}

	public void Tick(float delta)
	{
		TickRegen(delta);
		TickGrowth(delta);
		TickBreeding(delta);
	}

	private void TickRegen(float delta)
	{
		if (!_owner.IsPlayerOwned || DungeonFactionBuffs.LivingRegenPerSecond <= 0f || _owner.Health >= _owner.MaxHealth)
			return;
		_regenAccumulator += DungeonFactionBuffs.LivingRegenPerSecond * delta;
		int heal = (int)_regenAccumulator;
		if (heal <= 0)
			return;
		_regenAccumulator -= heal;
		HealthRestored?.Invoke(heal);
	}

	private void TickGrowth(float delta)
	{
		if (!LivingBuildingRules.IsLivingBuilding(_owner.TypeId) || !IsGrowthEligible())
			return;
		_growthAccumulator += delta;
		float interval = LivingBuildingRules.GetGrowthInterval(_owner.TypeId, GetKingLevel());
		if (_growthAccumulator < interval)
			return;
		_growthAccumulator = 0f;
		AutoGrow();
	}

	private bool IsGrowthEligible()
	{
		GameManager gameManager = AdapterRegistry.Resolve<GameManager>();
		if (gameManager == null || gameManager.CurrentState != GameManager.GameState.Playing || !gameManager.IsCombat)
			return false;
		if (LivingBuildingRules.GetNextInLine(_owner.TypeId) == null)
			return false;
		return GetKingLevel() >= LivingBuildingRules.GetRequiredKingLevel(_owner.TypeId);
	}

	private int GetKingLevel()
	{
		Castle castle = _owner.GetCastle();
		if (castle == null)
			return 1;
		return DungeonCoreRules.GetCoreLevel(castle.GetBuildingStates());
	}

	private void AutoGrow()
	{
		string nextTypeId = LivingBuildingRules.GetNextInLine(_owner.TypeId);
		Castle castle = _owner.GetCastle();
		if (nextTypeId == null || castle == null)
			return;
		UpgradeBuildingFactory factory = new(castle);
		factory.Create(nextTypeId, _owner.AnchorGridX, _owner.AnchorGridY, _owner.Health / (float)_owner.MaxHealth);
		factory.Destroy(_owner.Bridge);
		GameManager.Get<AdjacencyService>()?.RefreshCastle(castle.GetBuildingStates());
		Upgraded?.Invoke();
	}

	private void TickBreeding(float delta)
	{
		if (!DungeonFactionBuffs.AutoBreedEnabled || !LivingBuildingRules.IsLivingBuilding(_owner.TypeId) || !_owner.CanWork)
			return;
		_breedAccumulator += delta;
		if (_breedAccumulator < LivingBuildingRules.BreedIntervalSeconds)
			return;
		_breedAccumulator = 0f;
		TryBreedFleshNest();
	}

	private void TryBreedFleshNest()
	{
		Castle castle = _owner.GetCastle();
		if (castle == null)
			return;
		BuildingSystem system = AdapterRegistry.Resolve<BuildingSystem>();
		if (system.TryPlaceForCastle(castle, "FleshNest", _owner.AnchorGridX - 1, _owner.AnchorGridY))
			return;
		if (system.TryPlaceForCastle(castle, "FleshNest", _owner.AnchorGridX + 1, _owner.AnchorGridY))
			return;
		if (system.TryPlaceForCastle(castle, "FleshNest", _owner.AnchorGridX, _owner.AnchorGridY - 1))
			return;
		if (system.TryPlaceForCastle(castle, "FleshNest", _owner.AnchorGridX, _owner.AnchorGridY + 1))
			return;
	}
}
