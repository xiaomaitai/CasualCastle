// 仪式规则：仪式目录、宿主判定与研习仪式
using CasualCastle.Domain.Shared;
using System.Collections.Generic;

namespace CasualCastle.Domain.Building;

public static class RitualRules
{
	public const string DarkMagicStudyId = "dark_magic_study";
	public const string BlackMagicChargeId = "black_magic_charge";
	public const string UndeadSummonId = "undead_summon";
	public const string BoneReinforceId = "bone_reinforce";
	public const string DeathAngelSummonId = "death_angel_summon";
	public const string NecromancyStudyId = "necromancy_study";
	public const string AltarHostTypeId = BuildingTypeIds.DarkAltar;
	public const string RitualCircleTypePrefix = BuildingTypeIds.RitualCirclePrefix;

	private static readonly RitualDef[] AllRituals =
	{
		new()
		{
			Id = DarkMagicStudyId,
			DisplayName = "黑魔法研习",
			RequiredCoreLevel = 1,
			CostType = RitualCostType.BlackMagic,
			CostAmount = DarkMagicStudyRules.BaseCost,
			DurationSeconds = DarkMagicStudyRules.DurationSeconds,
			BasicReward = new RitualReward(),
			CoreReward = new RitualReward { CoreLevelUp = true }
		},
		new()
		{
			Id = BlackMagicChargeId,
			DisplayName = "黑魔法充能",
			RequiredCoreLevel = 1,
			CostType = RitualCostType.BlackMagic,
			CostAmount = 20,
			DurationSeconds = 10f,
			BasicReward = new RitualReward { GoldAmount = 30 },
			CoreReward = new RitualReward { GoldAmount = 30 }
		},
		new()
		{
			Id = UndeadSummonId,
			DisplayName = "亡灵召唤",
			RequiredCoreLevel = 1,
			CostType = RitualCostType.Gold,
			CostAmount = 30,
			DurationSeconds = 10f,
			BasicReward = new RitualReward { SummonUnitTypeId = "Skeleton", SummonCount = 3 },
			CoreReward = new RitualReward { SummonUnitTypeId = "Skeleton", SummonCount = 5 }
		},
		new()
		{
			Id = BoneReinforceId,
			DisplayName = "骸骨强化",
			RequiredCoreLevel = 2,
			CostType = RitualCostType.BlackMagic,
			CostAmount = 60,
			DurationSeconds = 20f,
			BasicReward = new RitualReward { SummonUnitTypeId = "Zombie", SummonCount = 1 },
			CoreReward = new RitualReward { UndeadMaxHealthBonus = 10 }
		},
		new()
		{
			Id = DeathAngelSummonId,
			DisplayName = "死亡天使降临",
			RequiredCoreLevel = 3,
			CostType = RitualCostType.BlackMagic,
			CostAmount = 80,
			DurationSeconds = 25f,
			BasicReward = new RitualReward { SummonUnitTypeId = "SkeletonWarrior", SummonCount = 2 },
			CoreReward = new RitualReward { SummonUnitTypeId = "DeathAngel", SummonCount = 1 }
		},
		new()
		{
			Id = NecromancyStudyId,
			DisplayName = "亡灵巫术研习",
			RequiredCoreLevel = 4,
			CostType = RitualCostType.BlackMagic,
			CostAmount = 100,
			DurationSeconds = 30f,
			BasicReward = new RitualReward { SummonUnitTypeId = "BloodKnight", SummonCount = 1 },
			CoreReward = new RitualReward { UndeadDamageMultiplier = 0.10f }
		}
	};

	public static IReadOnlyList<RitualDef> GetAll()
	{
		return AllRituals;
	}

	public static bool IsStudyRitual(string ritualId)
	{
		return ritualId == DarkMagicStudyId;
	}

	public static RitualDef Get(string ritualId)
	{
		foreach (RitualDef ritual in AllRituals)
		{
			if (ritual.Id == ritualId)
				return ritual;
		}
		return null;
	}

	public static bool CanHostRitual(string typeId)
	{
		return typeId == AltarHostTypeId || IsRitualCircleHost(typeId);
	}

	public static bool IsRitualCircleHost(string typeId)
	{
		return typeId != null && typeId.StartsWith(RitualCircleTypePrefix);
	}
}