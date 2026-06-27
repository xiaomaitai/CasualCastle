namespace CasualCastle.Domain.Battle;

// Domain combat stats — no Godot dependency.
// Can be constructed from BuildingDefinition overrides or default values.
public class SoldierData
{
	public int Health { get; set; } = 30;
	public int Damage { get; set; } = 10;
	public float Speed { get; set; } = 80f;
	public float AttackRange { get; set; } = 30f;
	public float AttackCooldown { get; set; } = 1f;
	public bool HasNightCombat { get; set; }
}
