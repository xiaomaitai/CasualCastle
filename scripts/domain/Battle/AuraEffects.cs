// 光环结算结果：攻速/移速/攻击/回血/斩杀/护甲/减伤加成
namespace CasualCastle.Domain.Battle;

public class AuraEffects
{
	public float AttackSpeedMultiplier = 1f;
	public float MoveSpeedMultiplier = 1f;
	public float AttackDamageMultiplier = 1f;
	public float RegenPerSecond;
	public float ExecuteDamageMultiplier = 1f;
	public int ArmorBonus;
	public float LifestealFraction;
	public float DispelResistFraction;
	public float DamageReductionMultiplier = 1f;
}
