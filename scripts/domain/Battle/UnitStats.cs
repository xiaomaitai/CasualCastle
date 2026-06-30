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
	public float DisplaySize => Size switch
	{
		UnitSize.Small => 40f,
		UnitSize.Medium => 60f,
		UnitSize.Large => 80f,
		UnitSize.Huge => 120f,
		_ => 60f
	};
	public float CollisionRadius => Size switch
	{
		UnitSize.Small => 16f,
		UnitSize.Medium => 24f,
		UnitSize.Large => 32f,
		UnitSize.Huge => 48f,
		_ => 24f
	};

	public uint UnitColor { get; init; }
}
