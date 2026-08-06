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
	public void DayStart_TimerHold_IsOneSecond()
	{
		Assert.Equal(1f, GameRules.DayStartTimerHoldSeconds);
	}
}
