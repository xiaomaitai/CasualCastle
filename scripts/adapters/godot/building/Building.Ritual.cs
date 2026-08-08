// 建筑仪式：开始、计时、奖励结算与核心升级
using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Battle;
using CasualCastle.Domain.Building;
using CasualCastle.Domain.Shared;
using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class Building : Area2D
{
	public bool TryStartRitual(string ritualId)
	{
		if (_ritualService == null || _activeRitual != null || CastleRef == null)
			return false;
		if (!RitualRules.CanHostRitual(TypeId) || !IsPlayerOwned)
			return false;

		int coreLevel = RitualCircleRules.GetCoreLevel(CastleRef.GetBuildingStates());
		bool isCoreHosted = RitualRules.IsRitualCircleHost(TypeId);
		BlackMagicService blackMagic = GameManager.Get<BlackMagicService>();
		_activeRitual = _ritualService.TryStartRitual(
			ritualId, isCoreHosted, coreLevel,
			_shop.TrySpendGold,
			amount => blackMagic != null && blackMagic.TrySpend(amount));
		if (_activeRitual == null)
			return false;

		UpdateWorkCycle();
		UpdateStateIcon();
		return true;
	}

	private void TickRitual(float delta)
	{
		if (_activeRitual == null || _gameManager == null || _gameManager.CurrentState != GameManager.GameState.Playing)
			return;

		_ritualService.Tick(_activeRitual, delta);
		if (_activeRitual.IsComplete)
			CompleteRitual();
	}

	private void CompleteRitual()
	{
		RitualReward reward = _ritualService.GetReward(_activeRitual);
		ApplyRitualReward(reward);
		bool isCoreHosted = RitualRules.IsRitualCircleHost(TypeId);
		_activeRitual = null;
		if (isCoreHosted)
			CallDeferred(nameof(UpgradeCircleAfterRitual));
		else
			UpdateWorkCycle();
	}

	private void ApplyRitualReward(RitualReward reward)
	{
		if (reward == null)
			return;
		if (reward.GoldAmount > 0 && _shop != null)
			_shop.AddGold(reward.GoldAmount);
		if (!string.IsNullOrEmpty(reward.SummonUnitTypeId) && reward.SummonCount > 0)
			SpawnUnitType(reward.SummonUnitTypeId, reward.SummonCount);
		if (reward.UndeadMaxHealthBonus > 0)
			ApplyUndeadHealthBonus(reward.UndeadMaxHealthBonus);
	}

	private void ApplyUndeadHealthBonus(int amount)
	{
		UndeadFactionBuffs.AddUndeadMaxHealthBonus(amount);
		IFieldUnitRepository fieldRepo = GameManager.Get<IFieldUnitRepository>();
		if (fieldRepo == null)
			return;
		foreach (IUnitHandle unit in fieldRepo.AllUnits)
		{
			if (!unit.IsAlive || unit.IsPlayerUnit != _isPlayerBuilding || !UndeadRules.IsUndead(unit.Race))
				continue;
			unit.AddMaxHealthBonus(amount);
			if (unit is UnitLogic logic)
				logic.RefreshHealthVisual();
		}
	}

	private void UpgradeCircleAfterRitual()
	{
		string nextTypeId = RitualCircleRules.GetNextLevelTypeId(TypeId);
		if (nextTypeId == null)
		{
			UpdateWorkCycle();
			return;
		}

		CastleRef.ReplaceHeart(nextTypeId, Health);
		Sfx.Play(SfxEvents.Upgrade);
	}

}
