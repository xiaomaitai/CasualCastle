using CasualCastle.Domain.Building;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class RepairRulesTests
{
	[Fact]
	public void CanRepair_AllConditionsMet_ReturnsTrue()
	{
		Assert.True(RepairRules.CanRepair(50, 100, false, true, false, true, true));
	}

	[Fact]
	public void CanRepair_HealthAtMax_ReturnsFalse()
	{
		Assert.False(RepairRules.CanRepair(100, 100, false, true, false, true, true));
	}

	[Fact]
	public void CanRepair_IsCore_ReturnsFalse()
	{
		Assert.False(RepairRules.CanRepair(50, 100, true, true, false, true, true));
	}

	[Fact]
	public void CanRepair_NotPlayerOwned_ReturnsFalse()
	{
		Assert.False(RepairRules.CanRepair(50, 100, false, false, false, true, true));
	}

	[Fact]
	public void CanRepair_HasEnemyOnTop_ReturnsFalse()
	{
		Assert.False(RepairRules.CanRepair(50, 100, false, true, true, true, true));
	}

	[Fact]
	public void CanRepair_NotPlaying_ReturnsFalse()
	{
		Assert.False(RepairRules.CanRepair(50, 100, false, true, false, false, true));
	}

	[Fact]
	public void CanRepair_Day_ReturnsFalse()
	{
		Assert.False(RepairRules.CanRepair(50, 100, false, true, false, true, false));
	}

	[Fact]
	public void GetRepairCost_CalculatesCorrectly()
	{
		int cost = RepairRules.GetRepairCost(100, 30, 1);
		Assert.Equal(70, cost);
	}

	[Fact]
	public void GetRepairCost_FullHealth_ReturnsZero()
	{
		int cost = RepairRules.GetRepairCost(100, 100, 1);
		Assert.Equal(0, cost);
	}
}
