// 技能上下文：触发条件与光环加成快照
using System.Collections.Generic;

namespace CasualCastle.Domain.Battle;

public class GameContext
{
	public float CurrentHpRatio;
	public string Race;
	public bool IsFurious;
	public HashSet<string> NearbyAllyRaces;
	public Dictionary<string, int> NearbyRaceCounts;
	public bool TargetIsIsolated;
	public float AuraAttackSpeedMultiplier = 1f;
	public float AuraMoveSpeedMultiplier = 1f;
	public float AuraAttackDamageMultiplier = 1f;
	public float AuraRegenPerSecond;
	public float AuraExecuteDamageMultiplier = 1f;
	public int AuraArmorBonus;
	public float AuraLifestealFraction;
	public float AuraDispelResistFraction;
}
