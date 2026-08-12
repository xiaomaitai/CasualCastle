// 魔王技能池：四邪神技能归类与升级三选一抽取
using System;
using System.Collections.Generic;
using System.Linq;

namespace CasualCastle.Domain.Battle;

public static class DemonKingSkillPool
{
	public const int ChoiceCount = 3;

	public const string Khorne = "khorne";
	public const string Tzeentch = "tzeentch";
	public const string Slaanesh = "slaanesh";
	public const string Nurgle = "nurgle";

	public static readonly string[] KhorneSkills = { "flame_slash", "blood_rage", "khorne_axe", "war_horn", "summon_bloodletter", "khorne_avatar" };
	public static readonly string[] TzeentchSkills = { "spike_trap_skill", "poison_trap_skill", "fire_trap_skill", "mana_storm", "tzeentch_rune", "teleport_trap", "labyrinth" };
	public static readonly string[] SlaaneshSkills = { "grand_hall", "feast", "charm_servitude", "seductive_voice", "slaanesh_grace", "paradise_realm" };
	public static readonly string[] NurgleSkills = { "regen_aura_skill", "living_building", "nurgle_pus", "fast_breed", "plague_spread", "life_link", "nurgle_garden" };

	public static List<string> GenerateChoices(Random random, int level, IEnumerable<string> learned)
	{
		List<string> pool = GetEligibleSkills(level);
		List<string> choices = new();
		while (choices.Count < ChoiceCount && pool.Count > 0)
		{
			int index = random.Next(pool.Count);
			string skillId = pool[index];
			pool.RemoveAt(index);
			if (learned == null || !learned.Contains(skillId))
				choices.Add(skillId);
		}
		return choices;
	}

	public static string GetGod(string skillId)
	{
		if (Contains(KhorneSkills, skillId))
			return Khorne;
		if (Contains(TzeentchSkills, skillId))
			return Tzeentch;
		if (Contains(SlaaneshSkills, skillId))
			return Slaanesh;
		if (Contains(NurgleSkills, skillId))
			return Nurgle;
		return "";
	}

	public static bool IsUltimate(string skillId)
	{
		return skillId == "khorne_avatar" || skillId == "labyrinth"
			|| skillId == "paradise_realm" || skillId == "nurgle_garden";
	}

	public static bool IsKnownSkill(string skillId)
	{
		return GetGod(skillId).Length > 0;
	}

	public static int GetNormalSkillCount()
	{
		return KhorneSkills.Length - 1 + TzeentchSkills.Length - 1
			+ SlaaneshSkills.Length - 1 + NurgleSkills.Length - 1;
	}

	private static List<string> GetEligibleSkills(int level)
	{
		List<string> pool = new();
		if (level < 2)
			return pool;
		AddNormalSkills(pool);
		if (level >= 5)
			AddUltimateSkills(pool);
		return pool;
	}

	private static void AddNormalSkills(List<string> pool)
	{
		AddNormals(pool, KhorneSkills);
		AddNormals(pool, TzeentchSkills);
		AddNormals(pool, SlaaneshSkills);
		AddNormals(pool, NurgleSkills);
	}

	private static void AddUltimateSkills(List<string> pool)
	{
		AddSkill(pool, KhorneSkills[KhorneSkills.Length - 1]);
		AddSkill(pool, TzeentchSkills[TzeentchSkills.Length - 1]);
		AddSkill(pool, SlaaneshSkills[SlaaneshSkills.Length - 1]);
		AddSkill(pool, NurgleSkills[NurgleSkills.Length - 1]);
	}

	private static void AddNormals(List<string> pool, string[] skills)
	{
		for (int i = 0; i < skills.Length - 1; i++)
			AddSkill(pool, skills[i]);
	}

	private static void AddSkill(List<string> pool, string skillId)
	{
		pool.Add(skillId);
	}

	private static bool Contains(string[] skills, string skillId)
	{
		foreach (string skill in skills)
		{
			if (skill == skillId)
				return true;
		}
		return false;
	}
}