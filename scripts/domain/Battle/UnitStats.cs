namespace CasualCastle.Domain.Battle;

public class UnitStats
{
	public string TypeId { get; init; }
	public UnitSize Size { get; init; }
	public AttackType AttackType { get; init; }
	public DamageType DamageType { get; init; }
	public ArmorType ArmorType { get; init; }
	public int Health { get; init; }
	public int Damage { get; init; }
	public float Speed { get; init; }
	public float AttackRange { get; init; }
	public float AttackCooldown { get; init; }
	public float VisionRange { get; init; } = 170f;
	public bool HasNightCombat { get; init; }
	public float CollisionRadius => Size switch
	{
		UnitSize.Small => 18f,
		UnitSize.Medium => 27f,
		UnitSize.Large => 36f,
		UnitSize.Huge => 54f,
		_ => 27f
	};

	public int UnitCost { get; init; }
	public string Race { get; init; }
}
