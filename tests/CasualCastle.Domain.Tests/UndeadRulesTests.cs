using CasualCastle.Domain.Battle;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class UndeadRulesTests
{
	[Fact]
	public void IsUndead_UndeadRace_ReturnsTrue()
	{
		Assert.True(UndeadRules.IsUndead("Undead"));
	}

	[Fact]
	public void IsUndead_OtherRace_ReturnsFalse()
	{
		Assert.False(UndeadRules.IsUndead("Human"));
	}

	[Fact]
	public void IsNonLiving_MechanicalAndElemental_ReturnsTrue()
	{
		Assert.True(UndeadRules.IsNonLiving("Mechanical"));
		Assert.True(UndeadRules.IsNonLiving("Elemental"));
		Assert.True(UndeadRules.IsNonLiving("Golem"));
		Assert.True(UndeadRules.IsNonLiving("Construct"));
	}

	[Fact]
	public void IsNonLiving_LivingRace_ReturnsFalse()
	{
		Assert.False(UndeadRules.IsNonLiving("Undead"));
		Assert.False(UndeadRules.IsNonLiving("Human"));
	}

	[Fact]
	public void ApplyDispelMultiplier_HolyVsUndead_DoublesDamage()
	{
		int result = UndeadRules.ApplyDispelMultiplier(10, DamageType.Holy, "Undead");

		Assert.Equal(20, result);
	}

	[Fact]
	public void ApplyDispelMultiplier_HolyVsLiving_Unchanged()
	{
		int result = UndeadRules.ApplyDispelMultiplier(10, DamageType.Holy, "Human");

		Assert.Equal(10, result);
	}

	[Fact]
	public void ApplyDispelMultiplier_HalfResist_ReducesDispelBonus()
	{
		int result = UndeadRules.ApplyDispelMultiplier(10, DamageType.Holy, "Undead", 0.50f);

		Assert.Equal(15, result);
	}

	[Fact]
	public void ApplyDispelMultiplier_FullResist_RemovesDispelBonus()
	{
		int result = UndeadRules.ApplyDispelMultiplier(10, DamageType.Holy, "Undead", 1f);

		Assert.Equal(10, result);
	}

	[Fact]
	public void ApplyDispelMultiplier_SlashVsUndead_Unchanged()
	{
		int result = UndeadRules.ApplyDispelMultiplier(10, DamageType.Slash, "Undead");

		Assert.Equal(10, result);
	}
}