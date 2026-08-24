// 技能效果类型常量：光环加成与命中/治疗特效分类标识
namespace CasualCastle.Domain.Battle;

public static class AuraBonusTypes
{
	public const string AttackSpeed = "attack_speed";
	public const string MoveSpeed = "move_speed";
	public const string AttackDamage = "attack_damage";
	public const string Regen = "regen";
	public const string Execute = "execute";
	public const string Armor = "armor";
	public const string Lifesteal = "lifesteal";
	public const string DispelResist = "dispel_resist";
	public const string DamageReduction = "damage_reduction";
}

public static class HitEffectTypes
{
	public const string Slow = "slow";
	public const string Poison = "poison";
	public const string Explosion = "explosion";
}

public static class HealEffectTypes
{
	public const string Shield = "shield";
	public const string CooldownReduction = "cooldown_reduction";
	public const string Burn = "burn";
}
