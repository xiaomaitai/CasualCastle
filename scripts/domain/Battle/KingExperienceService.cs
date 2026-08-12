// 魔王经验服务：阵亡/击杀/魔晶注入经验并升级
using System;

namespace CasualCastle.Domain.Battle;

public sealed class KingExperienceService
{
	public event Action<int> LevelUp;
	public event Action<int> ExperienceChanged;

	public int Level { get; private set; } = 1;

	public int Experience { get; private set; }

	public float KillXpMultiplier { get; set; } = 1f;

	public void OnUnitDied(bool friendlyToDungeon)
	{
		AddExperience(friendlyToDungeon ? DemonKingRules.FriendlyDeathXp : DemonKingRules.EnemyDeathXp);
	}

	public void OnKingKill()
	{
		AddExperience((int)(DemonKingRules.KingKillXp * KillXpMultiplier));
	}

	public void OnCrystalUsed()
	{
		AddExperience(DemonKingRules.CrystalXp);
	}

	public void OnSmallCrystalMined()
	{
		AddExperience(DemonKingRules.SmallCrystalXp);
	}

	public void AddExperience(int amount)
	{
		if (amount <= 0 || Level >= 5)
			return;
		Experience += amount;
		ExperienceChanged?.Invoke(Experience);
		while (Level < 5 && Experience >= GetRequiredExperience(Level))
		{
			Experience -= GetRequiredExperience(Level);
			Level++;
			LevelUp?.Invoke(Level);
		}
	}

	public int GetCumulativeExperience()
	{
		int total = Experience;
		for (int i = 1; i < Level; i++)
			total += GetRequiredExperience(i);
		return total;
	}

	public void Restore(int cumulativeExperience)
	{
		if (cumulativeExperience <= 0)
			return;
		Experience = cumulativeExperience;
		Level = 1;
		while (Level < 5 && Experience >= GetRequiredExperience(Level))
		{
			Experience -= GetRequiredExperience(Level);
			Level++;
		}
		ExperienceChanged?.Invoke(Experience);
	}

	public int GetRemainingExperienceToNextLevel()
	{
		if (Level >= 5)
			return 0;
		return GetRequiredExperience(Level) - Experience;
	}

	public void Reset()
	{
		Level = 1;
		Experience = 0;
		KillXpMultiplier = 1f;
	}

	private static int GetRequiredExperience(int level)
	{
		return DemonKingRules.GetRequiredExperienceForLevel(level);
	}
}