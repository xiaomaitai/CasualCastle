using CasualCastle.Domain.Battle;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class UnitStatsTests
{
	[Fact]
	public void CollisionRadius_Small_Returns18()
	{
		UnitStats stats = new() { TypeId = "test", Size = UnitSize.Small, AttackType = AttackType.Melee, DamageType = DamageType.Slash, ArmorType = ArmorType.Leather };
		Assert.Equal(18f, stats.CollisionRadius);
	}

	[Fact]
	public void CollisionRadius_Medium_Returns27()
	{
		UnitStats stats = new() { TypeId = "test", Size = UnitSize.Medium, AttackType = AttackType.Melee, DamageType = DamageType.Slash, ArmorType = ArmorType.Leather };
		Assert.Equal(27f, stats.CollisionRadius);
	}

	[Fact]
	public void CollisionRadius_Large_Returns36()
	{
		UnitStats stats = new() { TypeId = "test", Size = UnitSize.Large, AttackType = AttackType.Melee, DamageType = DamageType.Slash, ArmorType = ArmorType.Leather };
		Assert.Equal(36f, stats.CollisionRadius);
	}

	[Fact]
	public void CollisionRadius_Huge_Returns54()
	{
		UnitStats stats = new() { TypeId = "test", Size = UnitSize.Huge, AttackType = AttackType.Melee, DamageType = DamageType.Slash, ArmorType = ArmorType.Leather };
		Assert.Equal(54f, stats.CollisionRadius);
	}

	[Fact]
	public void ArmorValue_Default_IsZero()
	{
		UnitStats stats = new() { TypeId = "test", Size = UnitSize.Medium, AttackType = AttackType.Melee, DamageType = DamageType.Slash, ArmorType = ArmorType.Leather };
		Assert.Equal(0, stats.ArmorValue);
	}
}
