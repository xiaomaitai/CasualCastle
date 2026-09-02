// 阶段映射：对局阶段与战报阶段的唯一对应关系
using CasualCastle.Domain.Shared;
using System;

namespace CasualCastle.Domain.History;

public static class PhaseMapping
{
	public static BattlePhase ToBattlePhase(MatchPhase phase)
	{
		return phase switch
		{
			MatchPhase.Combat => BattlePhase.Day,
			MatchPhase.Shop => BattlePhase.Night,
			_ => throw new ArgumentOutOfRangeException(nameof(phase), phase, null),
		};
	}
}
