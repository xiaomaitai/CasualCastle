// 仪式服务：开始、计时与奖励
using System;

namespace CasualCastle.Domain.Building;

public class ActiveRitual
{
	public string RitualId { get; }
	public bool IsCoreHosted { get; }
	public float RemainingSeconds { get; set; }
	public float TotalSeconds { get; }

	public ActiveRitual(string ritualId, float totalSeconds, bool isCoreHosted)
	{
		RitualId = ritualId;
		TotalSeconds = totalSeconds;
		RemainingSeconds = totalSeconds;
		IsCoreHosted = isCoreHosted;
	}

	public bool IsComplete => RemainingSeconds <= 0f;
}

public class RitualService
{
	public ActiveRitual TryStartRitual(string ritualId, bool isCoreHosted, int coreLevel, Func<int, bool> trySpendGold, Func<int, bool> trySpendBlackMagic)
	{
		RitualDef def = RitualRules.Get(ritualId);
		if (def == null || coreLevel < def.RequiredCoreLevel)
			return null;

		bool spent = def.CostType switch
		{
			RitualCostType.Gold => trySpendGold(def.CostAmount),
			_ => trySpendBlackMagic(def.CostAmount)
		};
		if (!spent)
			return null;

		return new ActiveRitual(def.Id, def.DurationSeconds, isCoreHosted);
	}

	public void Tick(ActiveRitual ritual, float delta)
	{
		if (ritual.RemainingSeconds > 0f)
			ritual.RemainingSeconds -= delta;
	}

	public RitualReward GetReward(ActiveRitual ritual)
	{
		RitualDef def = RitualRules.Get(ritual.RitualId);
		if (def == null)
			return new RitualReward();
		return ritual.IsCoreHosted ? def.CoreReward : def.BasicReward;
	}
}