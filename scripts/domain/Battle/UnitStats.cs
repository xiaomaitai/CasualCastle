// 单位属性数据
namespace CasualCastle.Domain.Battle;

public class UnitStats
{
	public const float SmallCollisionRadius = 18f;
	public const float MediumCollisionRadius = 27f;
	public const float LargeCollisionRadius = 36f;
	public const float HugeCollisionRadius = 54f;
	public string TypeId { get; init; }
	public UnitSize Size { get; init; }
	public AttackType AttackType { get; init; }
	public DamageType DamageType { get; init; }
	public ArmorType ArmorType { get; init; }
	public int ArmorValue { get; init; }
	public int Health { get; init; }
	public int Damage { get; init; }
	public float Speed { get; init; }
	public float AttackRange { get; init; }
	public float AttackCooldown { get; init; }
	public float VisionRange { get; init; } = 170f;
	public float CollisionRadius => Size switch
	{
		UnitSize.Small => SmallCollisionRadius,
		UnitSize.Medium => MediumCollisionRadius,
		UnitSize.Large => LargeCollisionRadius,
		UnitSize.Huge => HugeCollisionRadius,
		_ => MediumCollisionRadius
	};

	public int UnitCost { get; init; }
	public string Race { get; init; }
	public bool CanDetectStealth { get; init; }
	public bool HasNightCombat { get; init; }
	public float HealAmount { get; init; }
	public float HealRange { get; init; }
	public float HealInterval { get; init; }
}
