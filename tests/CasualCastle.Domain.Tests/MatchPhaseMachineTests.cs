using CasualCastle.Domain.Shared;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class MatchPhaseMachineTests
{
	[Fact]
	public void StartMatch_ResetsToCombatWithNightZero()
	{
		MatchPhaseMachine machine = new();
		machine.SetNightIndex(5);
		machine.Advance();

		machine.StartMatch();

		Assert.Equal(MatchPhase.Combat, machine.CurrentPhase);
		Assert.Equal(0, machine.NightIndex);
		Assert.Equal(GameRules.CombatDurationSeconds, machine.PhaseTimeRemaining);
		Assert.Equal(GameRules.CombatStartTimerHoldSeconds, machine.HoldRemaining);
	}

	[Fact]
	public void Tick_DuringCombatHold_DoesNotDecreaseTimer()
	{
		MatchPhaseMachine machine = new();

		machine.Tick(GameRules.CombatStartTimerHoldSeconds * 0.5f);

		Assert.Equal(GameRules.CombatDurationSeconds, machine.PhaseTimeRemaining);
		Assert.Equal(GameRules.CombatStartTimerHoldSeconds * 0.5f, machine.HoldRemaining, 4);
	}

	[Fact]
	public void Tick_AfterHoldAndCombatExpiry_AdvancesToShop()
	{
		MatchPhaseMachine machine = new();
		machine.Tick(GameRules.CombatStartTimerHoldSeconds);
		machine.Tick(GameRules.CombatDurationSeconds);

		Assert.Equal(MatchPhase.Shop, machine.CurrentPhase);
		Assert.Equal(1, machine.NightIndex);
	}

	[Fact]
	public void Tick_WhenExpiredByOvershoot_AdvancesOnce()
	{
		MatchPhaseMachine machine = new();
		machine.Tick(GameRules.CombatStartTimerHoldSeconds);
		machine.Tick(GameRules.CombatDurationSeconds + 10f);

		Assert.Equal(MatchPhase.Shop, machine.CurrentPhase);
		Assert.Equal(1, machine.NightIndex);
	}

	[Fact]
	public void Tick_ShopPhase_DoesNotAdvance()
	{
		MatchPhaseMachine machine = new();
		machine.Advance();

		machine.Tick(100f);

		Assert.Equal(MatchPhase.Shop, machine.CurrentPhase);
		Assert.Equal(1, machine.NightIndex);
	}

	[Fact]
	public void Advance_ShopToCombine_SetsCombineTimer()
	{
		MatchPhaseMachine machine = new();
		machine.Advance();

		machine.Advance();

		Assert.Equal(MatchPhase.Combine, machine.CurrentPhase);
		Assert.Equal(GameRules.CombineDurationSeconds, machine.PhaseTimeRemaining);
		Assert.Equal(0f, machine.HoldRemaining);
		Assert.Equal(1, machine.NightIndex);
	}

	[Fact]
	public void Advance_CombineToCombat_DoesNotIncrementNight()
	{
		MatchPhaseMachine machine = new();
		machine.Advance();
		machine.Advance();

		machine.Advance();

		Assert.Equal(MatchPhase.Combat, machine.CurrentPhase);
		Assert.Equal(1, machine.NightIndex);
		Assert.Equal(GameRules.CombatDurationSeconds, machine.PhaseTimeRemaining);
	}

	[Fact]
	public void PhaseChanged_RaisedWithPreviousAndNext()
	{
		MatchPhaseMachine machine = new();
		MatchPhase previous = MatchPhase.Combat;
		MatchPhase next = MatchPhase.Combat;
		machine.PhaseChanged += (oldPhase, newPhase) =>
		{
			previous = oldPhase;
			next = newPhase;
		};

		machine.Advance();

		Assert.Equal(MatchPhase.Combat, previous);
		Assert.Equal(MatchPhase.Shop, next);
	}
}
