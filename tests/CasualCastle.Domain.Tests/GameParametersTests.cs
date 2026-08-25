using CasualCastle.Domain.Shared;
using Xunit;

namespace CasualCastle.Domain.Tests;

[CollectionDefinition("GameParameters", DisableParallelization = true)]
public class GameParametersCollection
{
}

[Collection("GameParameters")]
public class GameParametersTests
{
	[Fact]
	public void Defaults_Match_GameRules()
	{
		Assert.Equal(GameRules.InitialGold, GameParameters.InitialGold);
		Assert.Equal(GameRules.CombatDurationSeconds, GameParameters.CombatDurationSeconds);
		Assert.Equal(GameRules.CombineDurationSeconds, GameParameters.CombineDurationSeconds);
		Assert.Equal(GameRules.CombatStartTimerHoldSeconds, GameParameters.CombatStartTimerHoldSeconds);
		Assert.Equal(GameRules.RepairGoldPerHealth, GameParameters.RepairGoldPerHealth);
	}

	[Fact]
	public void SetValue_IsReadBack()
	{
		GameParameters.InitialGold = 999;
		Assert.Equal(999, GameParameters.InitialGold);
		GameParameters.Reset();
	}

	[Fact]
	public void Reset_Restores_GameRulesDefaults()
	{
		GameParameters.InitialGold = 1;
		GameParameters.CombatDurationSeconds = 2f;
		GameParameters.CombineDurationSeconds = 3f;
		GameParameters.CombatStartTimerHoldSeconds = 4f;
		GameParameters.RepairGoldPerHealth = 5;
		GameParameters.Reset();
		Assert.Equal(GameRules.InitialGold, GameParameters.InitialGold);
		Assert.Equal(GameRules.CombatDurationSeconds, GameParameters.CombatDurationSeconds);
		Assert.Equal(GameRules.CombineDurationSeconds, GameParameters.CombineDurationSeconds);
		Assert.Equal(GameRules.CombatStartTimerHoldSeconds, GameParameters.CombatStartTimerHoldSeconds);
		Assert.Equal(GameRules.RepairGoldPerHealth, GameParameters.RepairGoldPerHealth);
	}
}