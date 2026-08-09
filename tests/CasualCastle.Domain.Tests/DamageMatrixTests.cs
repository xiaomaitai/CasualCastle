using CasualCastle.Domain.Battle;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class DamageMatrixTests
{
	[Fact]
	public void Unset_Pair_Defaults_To_One()
	{
		DamageMatrix dm = new();
		Assert.Equal(1.0f, dm.GetMultiplier(DamageType.Slash, ArmorType.Unarmored));
		Assert.Equal(1.0f, dm.GetMultiplier(DamageType.Magic, ArmorType.Chainmail));
	}

	[Fact]
	public void Set_And_Get_Works()
	{
		DamageMatrix dm = new();
		dm.Set(DamageType.Pierce, ArmorType.Leather, 1.5f);
		dm.Set(DamageType.Chaos, ArmorType.Unarmored, 0.7f);
		dm.Set(DamageType.Slash, ArmorType.Holy, 0.1f);
		Assert.Equal(1.5f, dm.GetMultiplier(DamageType.Pierce, ArmorType.Leather));
		Assert.Equal(0.7f, dm.GetMultiplier(DamageType.Chaos, ArmorType.Unarmored));
		Assert.Equal(0.1f, dm.GetMultiplier(DamageType.Slash, ArmorType.Holy));
	}

	[Fact]
	public void Set_Overwrites_Previous_Value()
	{
		DamageMatrix dm = new();
		dm.Set(DamageType.Slash, ArmorType.Plate, 0.7f);
		dm.Set(DamageType.Slash, ArmorType.Plate, 1.5f);
		Assert.Equal(1.5f, dm.GetMultiplier(DamageType.Slash, ArmorType.Plate));
	}

	[Fact]
	public void CalculateReduction_100Armor_Returns50Percent()
	{
		double reduction = DamageMatrix.CalculateReduction(100);
		Assert.Equal(0.5, reduction, 6);
	}

	[Fact]
	public void CalculateReduction_200Armor_Returns66Percent()
	{
		double reduction = DamageMatrix.CalculateReduction(200);
		Assert.Equal(0.6666666666666666, reduction, 4);
	}

	[Fact]
	public void PercentScale_IsOneHundred()
	{
		Assert.Equal(100, DamageMatrix.PercentScale);
	}

	[Fact]
	public void PercentToMultiplier_ConvertsPercentToMultiplier()
	{
		Assert.Equal(1.5f, DamageMatrix.PercentToMultiplier(150));
		Assert.Equal(1.0f, DamageMatrix.PercentToMultiplier(100));
		Assert.Equal(0.7f, DamageMatrix.PercentToMultiplier(70));
		Assert.Equal(0.1f, DamageMatrix.PercentToMultiplier(10));
	}

	[Fact]
	public void MultiplierToPercent_ConvertsMultiplierToPercent()
	{
		Assert.Equal(150, DamageMatrix.MultiplierToPercent(1.5f));
		Assert.Equal(100, DamageMatrix.MultiplierToPercent(1.0f));
		Assert.Equal(70, DamageMatrix.MultiplierToPercent(0.7f));
		Assert.Equal(10, DamageMatrix.MultiplierToPercent(0.1f));
	}

	[Fact]
	public void Percent_And_Multiplier_Converters_RoundTrip()
	{
		Assert.Equal(0.7f, DamageMatrix.PercentToMultiplier(DamageMatrix.MultiplierToPercent(0.7f)));
		Assert.Equal(1.5f, DamageMatrix.PercentToMultiplier(DamageMatrix.MultiplierToPercent(1.5f)));
		Assert.Equal(0.1f, DamageMatrix.PercentToMultiplier(DamageMatrix.MultiplierToPercent(0.1f)));
	}
}
