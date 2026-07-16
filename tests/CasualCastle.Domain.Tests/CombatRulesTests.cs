using CasualCastle.Domain.Battle;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class CombatRulesTests
{
	[Fact]
	public void CalculateDamage_PositiveBaseDamage_AppliesMultiplier()
	{
		DamageMatrix dm = new();
		int result = CombatRules.CalculateDamage(100, DamageType.Normal, ArmorType.Light, dm);
		Assert.Equal(100, result);
	}

	[Fact]
	public void CalculateDamage_ZeroBaseDamage_ReturnsZero()
	{
		DamageMatrix dm = new();
		int result = CombatRules.CalculateDamage(0, DamageType.Normal, ArmorType.Light, dm);
		Assert.Equal(0, result);
	}

	[Fact]
	public void CalculateDamage_NegativeBaseDamage_ReturnsZero()
	{
		DamageMatrix dm = new();
		int result = CombatRules.CalculateDamage(-5, DamageType.Normal, ArmorType.Light, dm);
		Assert.Equal(0, result);
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
	public void ApplyDamage_ZeroDamage_ReturnsCurrentHealth()
	{
		int result = CombatRules.ApplyDamage(50, 0);
		Assert.Equal(50, result);
	}

	[Fact]
	public void ApplyDamage_AlreadyDead_ReturnsCurrentHealth()
	{
		int result = CombatRules.ApplyDamage(0, 10);
		Assert.Equal(0, result);
	}

	[Fact]
	public void CanAttack_WhenTimerAtZero_ReturnsTrue()
	{
		bool result = CombatRules.CanAttack(0f);
		Assert.True(result);
	}

	[Fact]
	public void CanAttack_WhenTimerPositive_ReturnsFalse()
	{
		bool result = CombatRules.CanAttack(0.5f);
		Assert.False(result);
	}

	[Fact]
	public void TickCooldown_PositiveTimer_DecrementsByDelta()
	{
		float result = CombatRules.TickCooldown(0.5f, 0.2f);
		Assert.Equal(0.3f, result, 4);
	}

	[Fact]
	public void TickCooldown_ZeroTimer_ReturnsZero()
	{
		float result = CombatRules.TickCooldown(0f, 0.1f);
		Assert.Equal(0f, result);
	}

	[Fact]
	public void ResetCooldown_ReturnsCooldownDuration()
	{
		float result = CombatRules.ResetCooldown(1.5f);
		Assert.Equal(1.5f, result);
	}
}
