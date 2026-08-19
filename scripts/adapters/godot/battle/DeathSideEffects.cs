// 死亡副作用处理：按阵营结算黑魔法、地下城经验与血魔召唤
using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Battle;
using CasualCastle.Domain.Building;
using Godot;

public sealed class DeathSideEffects
{
	private readonly UnitLogic _owner;

	public DeathSideEffects(UnitLogic owner)
	{
		_owner = owner;
	}

	public void Process()
	{
		Shop shop = AdapterRegistry.Resolve<Shop>();
		if (shop == null)
			return;

		if (shop.FactionId == FactionDef.NecromancerId)
			GameManager.Get<BlackMagicService>()?.OnUnitDied(_owner.IsPlayerUnit, _owner.Handle?.Race);
		if (shop.FactionId == FactionDef.DungeonId)
			HandleDungeonDeath();
	}

	private void HandleDungeonDeath()
	{
		KingExperienceService xp = GameManager.Get<KingExperienceService>();
		if (xp == null)
			return;
		xp.OnUnitDied(_owner.IsPlayerUnit);
		if (_owner.IsPlayerUnit)
			return;
		if (_owner.Handle != null && _owner.Handle.UnitCost >= DemonKingRules.BossDropCostThreshold)
			xp.OnSmallCrystalMined();
		IUnitHandle killer = _owner.LastAttacker;
		if (killer != null && DemonKingRules.IsDemonKing(killer.Race))
		{
			xp.OnKingKill();
			TrySummonBloodletters(killer);
		}
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
