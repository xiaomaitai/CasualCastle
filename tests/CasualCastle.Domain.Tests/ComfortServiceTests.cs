using CasualCastle.Domain.Battle;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class ComfortServiceTests
{
	[Fact]
	public void InitialValue_IsMaxComfort()
	{
		ComfortService comfort = new();

		Assert.Equal(DemonKingRules.MaxComfort, comfort.Value);
		Assert.True(comfort.IsCombatReady);
	}

	[Fact]
	public void SpendAttack_ReducesComfort()
	{
		ComfortService comfort = new();

		comfort.SpendAttack();

		Assert.Equal(DemonKingRules.MaxComfort - DemonKingRules.AttackComfortCost, comfort.Value);
	}

	[Fact]
	public void SpendHit_ReducesComfort()
	{
		ComfortService comfort = new();

		comfort.SpendHit();

		Assert.Equal(DemonKingRules.MaxComfort - DemonKingRules.HitComfortCost, comfort.Value);
	}

	[Fact]
	public void TrySpend_WhenInsufficient_ReturnsFalse()
	{
		ComfortService comfort = new();
		comfort.Spend(DemonKingRules.MaxComfort);

		Assert.False(comfort.TrySpend(1f));
	}

	[Fact]
	public void Comfort_DoesNotGoBelowZero()
	{
		ComfortService comfort = new();
		comfort.Spend(DemonKingRules.MaxComfort + 50f);

		Assert.Equal(0f, comfort.Value);
		Assert.False(comfort.IsCombatReady);
	}

	[Fact]
	public void TickRestore_NearThroneAndNotInCombat_Recovers()
	{
		ComfortService comfort = new();
		comfort.Spend(50f);

		comfort.TickRestore(1f, true, false);

		Assert.Equal(50f + DemonKingRules.RestorePerSecondNearThrone, comfort.Value, 2);
	}

	[Fact]
	public void TickRestore_InCombat_DoesNotRecover()
	{
		ComfortService comfort = new();
		comfort.Spend(50f);

		comfort.TickRestore(1f, true, true);

		Assert.Equal(50f, comfort.Value, 2);
	}

	[Fact]
	public void TickDistancePenalty_OnlyAppliesBeyondPatrolRadius()
	{
		ComfortService comfort = new();

		comfort.TickDistancePenalty(1f, 120f, 200f);
		Assert.Equal(DemonKingRules.MaxComfort, comfort.Value);

		comfort.TickDistancePenalty(1f, 400f, 200f);
		Assert.True(comfort.Value < DemonKingRules.MaxComfort);
	}

	[Fact]
	public void Reset_RestoresMaxComfort()
	{
		ComfortService comfort = new();
		comfort.Spend(80f);

		comfort.Reset();

		Assert.Equal(DemonKingRules.MaxComfort, comfort.Value);
	}
}