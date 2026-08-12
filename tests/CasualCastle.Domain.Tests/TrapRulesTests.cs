using CasualCastle.Domain.Battle;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class TrapRulesTests
{
	[Theory]
	[InlineData("SpikeTrap", true)]
	[InlineData("PoisonTrap", true)]
	[InlineData("FireTrap", true)]
	[InlineData("TeleportTrap", true)]
	[InlineData("DemonGate", false)]
	public void IsTrap_MatchesTrapTypes(string typeId, bool expected)
	{
		Assert.Equal(expected, TrapRules.IsTrap(typeId));
	}

	[Fact]
	public void SpikeTrap_Config_DamagesWithoutPoison()
	{
		TrapConfig config = TrapRules.GetConfig(TrapRules.SpikeTrap);

		Assert.Equal(25, config.Damage);
		Assert.False(config.IsTeleport);
		Assert.Equal(0f, config.PoisonPerSecond);
	}

	[Fact]
	public void PoisonTrap_Config_AppliesPoison()
	{
		TrapConfig config = TrapRules.GetConfig(TrapRules.PoisonTrap);

		Assert.True(config.PoisonPerSecond > 0f);
		Assert.True(config.PoisonDuration > 0f);
	}

	[Fact]
	public void TeleportTrap_Config_Teleports()
	{
		TrapConfig config = TrapRules.GetConfig(TrapRules.TeleportTrap);

		Assert.True(config.IsTeleport);
		Assert.Equal(0, config.Damage);
	}

	[Fact]
	public void UnknownType_ReturnsEmptyConfig()
	{
		TrapConfig config = TrapRules.GetConfig("Unknown");

		Assert.Equal(0, config.Damage);
		Assert.False(config.IsTeleport);
	}
}