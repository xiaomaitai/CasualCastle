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
	public void CalculateReduction_ZeroArmor_ReturnsZero()
	{
		double reduction = DamageMatrix.CalculateReduction(0);
		Assert.Equal(0.0, reduction);
	}
}
