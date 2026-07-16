using CasualCastle.Domain.Battle;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class DamageMatrixTests
{
	[Fact]
	public void DefaultMatrix_ReturnsOneForAllCombinations()
	{
		DamageMatrix dm = new();
		Assert.Equal(1f, dm.GetMultiplier(DamageType.Normal, ArmorType.Light));
		Assert.Equal(1f, dm.GetMultiplier(DamageType.Pierce, ArmorType.Heavy));
		Assert.Equal(1f, dm.GetMultiplier(DamageType.Siege, ArmorType.Fortified));
		Assert.Equal(1f, dm.GetMultiplier(DamageType.Magic, ArmorType.Beast));
	}

	[Fact]
	public void LoadFrom_CustomMatrix_ReturnsCustomValues()
	{
		float[,] matrix =
		{
			{ 1.0f, 0.5f, 2.0f, 1.0f },
			{ 2.0f, 1.0f, 0.5f, 1.0f },
			{ 0.5f, 2.0f, 1.0f, 1.0f },
			{ 1.0f, 1.0f, 1.0f, 1.5f },
		};
		DamageMatrix dm = new();
		dm.LoadFrom(matrix);

		Assert.Equal(0.5f, dm.GetMultiplier(DamageType.Normal, ArmorType.Heavy));
		Assert.Equal(2.0f, dm.GetMultiplier(DamageType.Normal, ArmorType.Fortified));
		Assert.Equal(1.5f, dm.GetMultiplier(DamageType.Magic, ArmorType.Beast));
	}
}
