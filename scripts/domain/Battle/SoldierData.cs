namespace CasualCastle.Domain.Battle;

public class SoldierData
{
	public string TypeId { get; set; } = "Swordsman";
	public UnitSize Size { get; set; } = UnitSize.Medium;
	public AttackType AttackType { get; set; }
	public DamageType DamageType { get; set; }
	public ArmorType ArmorType { get; set; } = ArmorType.Light;
	public int Health { get; set; } = 30;
	public int Damage { get; set; } = 10;
	public float Speed { get; set; } = 350f;
	public float AttackRange { get; set; } = 125f;
	public float AttackCooldown { get; set; } = 1f;
	public bool HasNightCombat { get; set; }
	public uint UnitColor { get; set; } = 0xFF888888u;

	public float DisplaySize()
	{
		return Size switch
		{
			UnitSize.Small => 80f,
			UnitSize.Medium => 125f,
			UnitSize.Large => 170f,
			UnitSize.Huge => 250f,
			_ => 125f
		};
	}

	public float CollisionRadius()
	{
		return Size switch
		{
			UnitSize.Small => 35f,
			UnitSize.Medium => 50f,
			UnitSize.Large => 65f,
			UnitSize.Huge => 100f,
			_ => 50f
		};
	}

	public static SoldierData FromStats(UnitStats stats)
	{
		return new SoldierData
		{
			TypeId = stats.TypeId,
			Size = stats.Size,
			AttackType = stats.AttackType,
			DamageType = stats.DamageType,
			ArmorType = stats.ArmorType,
			Health = stats.Health,
			Damage = stats.Damage,
			Speed = stats.Speed,
			AttackRange = stats.AttackRange,
			AttackCooldown = stats.AttackCooldown,
			HasNightCombat = stats.HasNightCombat,
			UnitColor = stats.UnitColor,
		};
	}
}
