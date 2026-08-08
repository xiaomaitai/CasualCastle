using CasualCastle.Domain.Shared;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class GameRulesTests
{
	[Fact]
	public void CombinePhase_Duration_IsOneSecond()
	{
		Assert.Equal(1f, GameRules.CombineDurationSeconds);
	}

	[Fact]
	public void CombatPhase_Duration_IsSixtySeconds()
	{
		Assert.Equal(60f, GameRules.CombatDurationSeconds);
	}

	[Fact]
	public void CombatStart_TimerHold_IsOneSecond()
	{
		Assert.Equal(1f, GameRules.CombatStartTimerHoldSeconds);
	}
}
