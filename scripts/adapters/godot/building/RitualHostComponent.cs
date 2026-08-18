// 仪式宿主组件：研习仪式计时与完成奖励分发，核心升级经 Building 回调
using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Battle;
using CasualCastle.Domain.Building;
using System;

public sealed class RitualHostComponent
{
	public event Action CoreUpgradeRequested;

	private readonly Building _owner;
	private readonly BuildingRuntime _runtime;
	private readonly RitualService _ritualService;
	private readonly GameManager _gameManager;

	public RitualHostComponent(Building owner, BuildingRuntime runtime, RitualService ritualService, GameManager gameManager)
	{
		_owner = owner;
		_runtime = runtime;
		_ritualService = ritualService;
		_gameManager = gameManager;
	}

	public void Tick(float delta)
	{
		if (!_runtime.IsRitualInProgress || _gameManager == null
			|| _gameManager.CurrentState != GameManager.GameState.Playing
			|| !_gameManager.IsCombat)
			return;

		_ritualService.Tick(_runtime.ActiveRitual, delta);
		if (_runtime.ActiveRitual.IsComplete)
			Complete();
	}

	private void Complete()
	{
		RitualReward reward = _ritualService.Complete(_runtime.ActiveRitual);
		ApplyReward(reward);
		bool isCoreHosted = RitualRules.IsRitualCircleHost(_owner.TypeId);
		_runtime.CompleteRitual();
		if (reward.CoreLevelUp && isCoreHosted)
			CoreUpgradeRequested?.Invoke();
		else
			_owner.RefreshProductionCycle();
	}

	private void ApplyReward(RitualReward reward)
	{
		if (reward == null)
			return;
		if (reward.GoldAmount > 0)
		{
			Shop shop = AdapterRegistry.Resolve<Shop>();
			if (shop != null)
				shop.AddGold(reward.GoldAmount);
		}
		if (reward.BlackMagicAmount > 0)
		{
			BlackMagicService blackMagic = GameManager.Get<BlackMagicService>();
			if (blackMagic != null)
				blackMagic.Add(reward.BlackMagicAmount);
		}
		if (!string.IsNullOrEmpty(reward.SummonUnitTypeId) && reward.SummonCount > 0)
		{
			UnitSpawnService spawnService = AdapterRegistry.Resolve<UnitSpawnService>();
			if (spawnService != null)
				spawnService.SpawnUnitType(_owner, reward.SummonUnitTypeId, reward.SummonCount);
		}
		if (reward.UndeadMaxHealthBonus > 0)
			ApplyUndeadHealthBonus(reward.UndeadMaxHealthBonus);
		if (reward.UndeadDamageMultiplier > 0f)
			UndeadFactionBuffs.AddUndeadDamageMultiplier(reward.UndeadDamageMultiplier);
		if (!string.IsNullOrEmpty(reward.LearnedRitualId))
			RemoveStudyBookFromHand();
	}

	private void ApplyUndeadHealthBonus(int amount)
	{
		UndeadFactionBuffs.AddUndeadMaxHealthBonus(amount);
		IFieldUnitRepository fieldRepo = GameManager.Get<IFieldUnitRepository>();
		if (fieldRepo == null)
			return;
		foreach (IUnitHandle unit in fieldRepo.AllUnits)
		{
			if (!unit.IsAlive || unit.IsPlayerUnit != _owner.IsPlayerOwned || !UndeadRules.IsUndead(unit.Race))
				continue;
			unit.AddMaxHealthBonus(amount);
			if (unit is UnitLogic logic)
				logic.RefreshHealthVisual();
		}
	}

	private void RemoveStudyBookFromHand()
	{
		Hand hand = AdapterRegistry.Resolve<Hand>();
		if (hand == null || !_runtime.IsRitualInProgress)
			return;
		hand.TryRemoveCardById(_runtime.ActiveRitual.BookCardId);
	}
}
