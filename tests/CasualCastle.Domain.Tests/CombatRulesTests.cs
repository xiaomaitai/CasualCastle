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
	public void CalculateDamage_Neutral_50PercentReduction()
	{
		DamageMatrix dm = new();
		int result = CombatRules.CalculateDamage(100, DamageType.Slash, ArmorType.Unarmored, 100, dm);
		Assert.Equal(50, result);
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
		int result = CombatRules.CalculateDamage(100, DamageType.Chaos, ArmorType.Plate, 100, dm);
		Assert.Equal(75, result);
	}

	[Fact]
	public void CalculateDamage_Bad_With_Armor()
	{
		DamageMatrix dm = CreateMatrix();
		int result = CombatRules.CalculateDamage(100, DamageType.Slash, ArmorType.Plate, 100, dm);
		Assert.Equal(35, result);
	}

	[Fact]
	public void CalculateDamage_HolyArmor_Weakness()
	{
		DamageMatrix dm = CreateMatrix();
		int result = CombatRules.CalculateDamage(100, DamageType.Slash, ArmorType.Holy, 0, dm);
		Assert.Equal(10, result);
	}

	[Fact]
	public void CalculateDamage_HolyAttack_FullDamage()
	{
		DamageMatrix dm = CreateMatrix();
		int result = CombatRules.CalculateDamage(100, DamageType.Holy, ArmorType.Plate, 200, dm);
		Assert.Equal(33, result);
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

	[Fact]
	public void CanAttack_WhenTimerAtZero_ReturnsTrue()
	{
		Assert.True(CombatRules.CanAttack(0f));
	}

	[Fact]
	public void CanAttack_WhenTimerPositive_ReturnsFalse()
	{
		Assert.False(CombatRules.CanAttack(0.5f));
	}

	[Fact]
	public void TickCooldown_PositiveTimer_DecrementsByDelta()
	{
		float result = CombatRules.TickCooldown(0.5f, 0.2f);
		Assert.Equal(0.3f, result, 4);
	}

	[Fact]
	public void ResetCooldown_ReturnsCooldownDuration()
	{
		float result = CombatRules.ResetCooldown(1.5f);
		Assert.Equal(1.5f, result);
	}
}
