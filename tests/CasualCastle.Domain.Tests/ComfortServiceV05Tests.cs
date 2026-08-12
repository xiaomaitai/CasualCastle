using CasualCastle.Domain.Battle;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class ComfortServiceV05Tests
{
	[Fact]
	public void SetMaxComfort_ExpandsAndCapsRestore()
	{
		ComfortService comfort = new();
		comfort.Spend(50f);

		comfort.SetMaxComfort(DemonKingRules.MaxComfort + 50f);

		Assert.Equal(150f, comfort.MaxComfort);
		comfort.TickRestore(1f, true, false);
		Assert.True(comfort.Value <= 150f);
	}

	[Fact]
	public void CostMultiplier_DoublesAttackSpend()
	{
		ComfortService comfort = new();
		comfort.GlobalCostMultiplier = 2f;

		comfort.SpendAttack();

		Assert.Equal(DemonKingRules.MaxComfort - DemonKingRules.AttackComfortCost * 2f, comfort.Value);
	}

	[Fact]
	public void Reset_RestoresDefaults()
	{
		ComfortService comfort = new();
		comfort.SetMaxComfort(180f);
		comfort.GlobalCostMultiplier = 2f;
		comfort.Spend(50f);

		comfort.Reset();

		Assert.Equal(DemonKingRules.MaxComfort, comfort.MaxComfort);
		Assert.Equal(1f, comfort.GlobalCostMultiplier);
		Assert.Equal(DemonKingRules.MaxComfort, comfort.Value);
	}
}