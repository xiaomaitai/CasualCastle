// 对局阶段状态机：作战/合成/商店推进、计时与夜数递增规则
using System;

namespace CasualCastle.Domain.Shared;

public enum MatchPhase
{
	Combat,
	Combine,
	Shop
}

public class MatchPhaseMachine
{
	public MatchPhase CurrentPhase { get; private set; } = MatchPhase.Combat;
	public int NightIndex { get; private set; }
	public float PhaseTimeRemaining { get; private set; } = GameParameters.CombatDurationSeconds;
	public float HoldRemaining { get; private set; } = GameParameters.CombatStartTimerHoldSeconds;

	public event Action<MatchPhase, MatchPhase> PhaseChanged;

	public void StartMatch()
	{
		MatchPhase previous = CurrentPhase;
		CurrentPhase = MatchPhase.Combat;
		NightIndex = 0;
		ApplyPhaseTimers(MatchPhase.Combat);
		PhaseChanged?.Invoke(previous, MatchPhase.Combat);
	}

	public void Tick(float delta)
	{
		if (CurrentPhase == MatchPhase.Shop)
			return;
		if (HoldRemaining > 0f)
		{
			HoldRemaining -= delta;
			return;
		}
		if (CurrentPhase == MatchPhase.Combat || CurrentPhase == MatchPhase.Combine)
		{
			PhaseTimeRemaining -= delta;
			if (PhaseTimeRemaining <= 0f)
				Advance();
		}
	}

	public void Advance()
	{
		MatchPhase previous = CurrentPhase;
		MatchPhase next = GetNextPhase(previous);
		if (next == MatchPhase.Shop)
			NightIndex++;
		CurrentPhase = next;
		ApplyPhaseTimers(next);
		PhaseChanged?.Invoke(previous, next);
	}

	public void SetNightIndex(int index)
	{
		NightIndex = index;
	}

	private static MatchPhase GetNextPhase(MatchPhase phase)
	{
		return phase switch
		{
			MatchPhase.Combat => MatchPhase.Shop,
			MatchPhase.Shop => MatchPhase.Combine,
			_ => MatchPhase.Combat,
		};
	}

	private void ApplyPhaseTimers(MatchPhase phase)
	{
		if (phase == MatchPhase.Combine)
			PhaseTimeRemaining = GameParameters.CombineDurationSeconds;
		else if (phase == MatchPhase.Combat)
			PhaseTimeRemaining = GameParameters.CombatDurationSeconds;
		else
			PhaseTimeRemaining = 0f;
		HoldRemaining = phase == MatchPhase.Combat ? GameParameters.CombatStartTimerHoldSeconds : 0f;
	}
}
