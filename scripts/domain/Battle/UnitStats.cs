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
	public bool HasNightCombat { get; init; }

	public float DisplaySize => Size switch
	{
		UnitSize.Small => 80f,
		UnitSize.Medium => 125f,
		UnitSize.Large => 170f,
		UnitSize.Huge => 250f,
		_ => 125f
	};

	public float CollisionRadius => Size switch
	{
		UnitSize.Small => 35f,
		UnitSize.Medium => 50f,
		UnitSize.Large => 65f,
		UnitSize.Huge => 100f,
		_ => 50f
	};

	public uint UnitColor { get; init; }
}
