using CasualCastle.Domain.Battle;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class CombatRulesTests
{
	private static DamageMatrix CreateMatrix()
	{
		DamageMatrix dm = new();
		dm.Set(DamageType.Pierce, ArmorType.Leather, 1.5f);
		dm.Set(DamageType.Chaos, ArmorType.Unarmored, 0.7f);
		dm.Set(DamageType.Chaos, ArmorType.Plate, 1.5f);
		dm.Set(DamageType.Slash, ArmorType.Plate, 0.7f);
		dm.Set(DamageType.Slash, ArmorType.Holy, 0.1f);
		dm.Set(DamageType.Holy, ArmorType.Plate, 1.0f);
		return dm;
	}

	[Fact]
	public void CalculateDamage_ZeroArmor_FullDamage()
	{
		DamageMatrix dm = new();
		int result = CombatRules.CalculateDamage(100, DamageType.Slash, ArmorType.Unarmored, 0, dm);
		Assert.Equal(100, result);
	}

	[Fact]
	public void CalculateDamage_5Armor_About25PercentReduction()
	{
		DamageMatrix dm = new();
		int result = CombatRules.CalculateDamage(100, DamageType.Slash, ArmorType.Unarmored, 5, dm);
		Assert.Equal(75, result);
	}

	[Fact]
	public void CalculateDamage_10Armor_About40PercentReduction()
	{
		DamageMatrix dm = new();
		int result = CombatRules.CalculateDamage(100, DamageType.Slash, ArmorType.Unarmored, 10, dm);
		Assert.Equal(60, result);
	}

	[Fact]
	public void CalculateDamage_NegativeArmor_IncreasesDamage()
	{
		DamageMatrix dm = new();
		int result = CombatRules.CalculateDamage(100, DamageType.Slash, ArmorType.Unarmored, -5, dm);
		Assert.Equal(125, result);
	}

	[Fact]
	public void CalculateDamage_ExtremeArmor_NeverFullyReducesDamage()
	{
		DamageMatrix dm = new();
		int result = CombatRules.CalculateDamage(100, DamageType.Slash, ArmorType.Unarmored, 10000, dm);
		Assert.Equal(35, result);
	}

	[Fact]
	public void CalculateDamage_Good_Multiplier_150Percent()
	{
		DamageMatrix dm = CreateMatrix();
		int result = CombatRules.CalculateDamage(100, DamageType.Pierce, ArmorType.Leather, 0, dm);
		Assert.Equal(150, result);
	}

	[Fact]
	public void CalculateDamage_Bad_Multiplier_70Percent()
	{
		DamageMatrix dm = CreateMatrix();
		int result = CombatRules.CalculateDamage(100, DamageType.Chaos, ArmorType.Unarmored, 0, dm);
		Assert.Equal(70, result);
	}

	[Fact]
	public void CalculateDamage_Good_With_Armor()
	{
		DamageMatrix dm = CreateMatrix();
		int result = CombatRules.CalculateDamage(100, DamageType.Chaos, ArmorType.Plate, 10, dm);
		Assert.Equal(90, result);
	}

	[Fact]
	public void CalculateDamage_Bad_With_Armor()
	{
		DamageMatrix dm = CreateMatrix();
		int result = CombatRules.CalculateDamage(100, DamageType.Slash, ArmorType.Plate, 10, dm);
		Assert.Equal(42, result);
	}

	[Fact]
	public void CalculateDamage_HolyArmor_Weakness()
	{
		DamageMatrix dm = CreateMatrix();
		int result = CombatRules.CalculateDamage(100, DamageType.Slash, ArmorType.Holy, 0, dm);
		Assert.Equal(10, result);
	}

	[Fact]
	public void CalculateDamage_HolyAttack_HighArmor_ReturnsCapDamage()
	{
		DamageMatrix dm = CreateMatrix();
		int result = CombatRules.CalculateDamage(100, DamageType.Holy, ArmorType.Plate, 200, dm);
		Assert.Equal(35, result);
	}

	[Fact]
	public void CalculateDamage_ZeroBaseDamage_ReturnsZero()
	{
		DamageMatrix dm = new();
		int result = CombatRules.CalculateDamage(0, DamageType.Slash, ArmorType.Unarmored, 100, dm);
		Assert.Equal(0, result);
	}

	[Fact]
	public void CalculateDamage_MinimumDamageFloor()
	{
		DamageMatrix dm = CreateMatrix();
		int result = CombatRules.CalculateDamage(1, DamageType.Slash, ArmorType.Holy, 0, dm);
		Assert.Equal(1, result);
	}

	[Fact]
	public void ApplyDamage_ReducesHealthByDamage()
	{
		int result = CombatRules.ApplyDamage(100, 30);
		Assert.Equal(70, result);
	}

	[Fact]
	public void ApplyDamage_ClampsToZero()
	{
		int result = CombatRules.ApplyDamage(10, 50);
		Assert.Equal(0, result);
	}
}
