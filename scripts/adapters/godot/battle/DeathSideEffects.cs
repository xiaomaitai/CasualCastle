// 死亡副作用处理：按阵营结算黑魔法、地下城经验与血魔召唤，并显示资源掉落飘字
using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Battle;
using CasualCastle.Domain.Building;
using Godot;
using System.Collections.Generic;

public sealed class DeathSideEffects
{
	private const float DropTextStackStep = 24f;
	private static readonly Color BlackMagicGainColor = new(0.7f, 0.46f, 0.88f, 1f);
	private static readonly Color ExperienceGainColor = new(0.55f, 0.85f, 1f, 1f);

	private readonly UnitLogic _owner;
	private readonly List<(int Amount, Color Color)> _gains = new();

	public DeathSideEffects(UnitLogic owner)
	{
		_owner = owner;
	}

	public void Process()
	{
		Shop shop = AdapterRegistry.Resolve<Shop>();
		if (shop == null)
			return;

		_gains.Clear();
		if (shop.FactionId == FactionDef.NecromancerId)
			CollectBlackMagic();
		if (shop.FactionId == FactionDef.DungeonId)
			CollectDungeonDrops();
		ShowDrops();
	}

	private void CollectBlackMagic()
	{
		BlackMagicService blackMagic = GameManager.Get<BlackMagicService>();
		if (blackMagic == null)
			return;
		AddDrop(blackMagic.OnUnitDied(_owner.IsPlayerUnit, _owner.Handle?.Race), BlackMagicGainColor);
	}

	private void CollectDungeonDrops()
	{
		KingExperienceService xp = GameManager.Get<KingExperienceService>();
		if (xp == null)
			return;
		AddDrop(xp.OnUnitDied(_owner.IsPlayerUnit), ExperienceGainColor);
		if (!_owner.IsPlayerUnit)
			CollectEnemyDrops(xp);
	}

	private void CollectEnemyDrops(KingExperienceService xp)
	{
		if (_owner.Handle != null && _owner.Handle.UnitCost >= DemonKingRules.BossDropCostThreshold)
			AddDrop(xp.OnSmallCrystalMined(), ExperienceGainColor);
		IUnitHandle killer = _owner.LastAttacker;
		if (killer == null || !DemonKingRules.IsDemonKing(killer.Race))
			return;
		AddDrop(xp.OnKingKill(), ExperienceGainColor);
		TrySummonBloodletters(killer);
	}

	private void AddDrop(int amount, Color color)
	{
		if (amount <= 0)
			return;
		_gains.Add((amount, color));
	}

	private void ShowDrops()
	{
		if (_gains.Count == 0 || !DisplaySettingsManager.ShowDamageNumbersEnabled)
			return;
		Node battlefield = AdapterRegistry.Resolve<GameManager>()?.Battlefield;
		if (battlefield == null || _owner.Handle == null)
			return;
		Vector2 origin = new(
			GameCoordinatesAdapter.GameUnitsToPixels(_owner.Handle.GameX),
			GameCoordinatesAdapter.GameUnitsToPixels(_owner.Handle.GameY));
		for (int i = 0; i < _gains.Count; i++)
			FloatingText.SpawnGain(battlefield, origin + new Vector2(0f, -DropTextStackStep * i), _gains[i].Amount, _gains[i].Color);
	}

	private void TrySummonBloodletters(IUnitHandle killer)
	{
		if (killer.Skills == null || !killer.Skills.HasSkill("summon_bloodletter"))
			return;
		Node2D battlefield = AdapterRegistry.Resolve<GameManager>()?.Battlefield;
		if (battlefield == null)
			return;
		UnitSpawnService spawnService = AdapterRegistry.Resolve<UnitSpawnService>();
		if (spawnService == null)
			return;
		float kingX = GameCoordinatesAdapter.GameUnitsToPixels(killer.GameX);
		float kingY = GameCoordinatesAdapter.GameUnitsToPixels(killer.GameY);
		for (int i = 0; i < 2; i++)
		{
			float offsetX = i == 0 ? -50f : 50f;
			spawnService.SpawnUnitAt(battlefield, "Bloodletter", new Vector2(kingX + offsetX, kingY + 40f), true);
		}
	}
}
