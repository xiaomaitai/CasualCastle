// 技能定义：类型与配置 JSON
namespace CasualCastle.Domain.Battle;

public enum SkillType
{
	StatModifier,
	Aura,
	OnHit,
	OnHeal,
	Special
}

public enum StatModifierTrigger
{
	Always,
	LowHealth,
	OnDamaged,
	TargetIsolated,
	NearbyDiverse,
	NearbyRaceCount
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
	Summon,
	Regen,
	Ambush,
	PlagueBurst,
	Curse,
	ShadowNova,
	ManaStorm
}
public class SkillDef
{
	public string Id { get; init; }
	public string DisplayName { get; init; }
	public string Description { get; init; } = "";
	public SkillType Type { get; init; }
	public string ConfigJson { get; init; }
}
