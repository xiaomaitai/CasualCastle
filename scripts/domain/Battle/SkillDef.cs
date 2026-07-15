namespace CasualCastle.Domain.Battle;

public enum SkillType
{
	StatModifier,
	Aura,
	OnHit,
	Special
}

public enum StatModifierTrigger
{
	Always,
	LowHealth,
	TargetIsolated,
	NearbyDiverse
}

public enum AuraTarget
{
	NearbyAllies,
	NearbyEnemies,
	AllAllies,
	AllEnemies
}

public enum SpecialBehavior
{
	Stealth,
	Kiting,
	Sweep,
	Charge,
	NoBattleReport,
	Summon
}

public class SkillDef
{
	public string Id { get; init; }
	public string DisplayName { get; init; }
	public SkillType Type { get; init; }
	public string ConfigJson { get; init; }
}
