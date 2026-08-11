// 仪式服务：开始、计时、习得与奖励
using System;
using System.Collections.Generic;

namespace CasualCastle.Domain.Building;

public class ActiveRitual
{
	public string RitualId { get; }
	public bool IsCoreHosted { get; }
	public float RemainingSeconds { get; set; }
	public float TotalSeconds { get; }
	public string BookCardId { get; }
	public string BookRitualId { get; }

	public ActiveRitual(string ritualId, float totalSeconds, bool isCoreHosted, string bookCardId, string bookRitualId)
	{
		RitualId = ritualId;
		TotalSeconds = totalSeconds;
		RemainingSeconds = totalSeconds;
		IsCoreHosted = isCoreHosted;
		BookCardId = bookCardId;
		BookRitualId = bookRitualId;
	}

	public bool IsComplete => RemainingSeconds <= 0f;
}

public class RitualService
{
	private readonly HashSet<string> _learnedRituals = new();

	public int StudyCompletionCount { get; private set; }

	public ActiveRitual TryStartRitual(
		string ritualId, bool isCoreHosted, int coreLevel,
		Func<int, bool> trySpendGold, Func<int, bool> trySpendBlackMagic,
		string bookCardId = null, string bookRitualId = null)
	{
		RitualDef def = RitualRules.Get(ritualId);
		if (def == null || coreLevel < def.RequiredCoreLevel)
			return null;
		if (!RitualRules.IsStudyRitual(ritualId) && !IsLearned(ritualId))
			return null;

		if (RitualRules.IsStudyRitual(ritualId))
			return TryStartStudyRitual(def, isCoreHosted, coreLevel, trySpendBlackMagic, bookCardId, bookRitualId);

		return TryStartLearnedRitual(def, isCoreHosted, trySpendGold, trySpendBlackMagic);
	}

	private ActiveRitual TryStartStudyRitual(
		RitualDef def, bool isCoreHosted, int coreLevel, Func<int, bool> trySpendBlackMagic,
		string bookCardId, string bookRitualId)
	{
		RitualDef bookDef = bookRitualId == null ? null : RitualRules.Get(bookRitualId);
		if (bookCardId == null || bookDef == null || bookDef.RequiredCoreLevel > coreLevel || IsLearned(bookRitualId))
			return null;

		int cost = DarkMagicStudyRules.GetCost(StudyCompletionCount);
		if (trySpendBlackMagic == null || !trySpendBlackMagic(cost))
			return null;

		return new ActiveRitual(def.Id, def.DurationSeconds, isCoreHosted, bookCardId, bookRitualId);
	}

	private ActiveRitual TryStartLearnedRitual(
		RitualDef def, bool isCoreHosted, Func<int, bool> trySpendGold, Func<int, bool> trySpendBlackMagic)
	{
		bool spent = def.CostType switch
		{
			RitualCostType.Gold => trySpendGold(def.CostAmount),
			_ => trySpendBlackMagic(def.CostAmount)
		};
		if (!spent)
			return null;

		return new ActiveRitual(def.Id, def.DurationSeconds, isCoreHosted, null, null);
	}

	public void Tick(ActiveRitual ritual, float delta)
	{
		if (ritual.RemainingSeconds > 0f)
			ritual.RemainingSeconds -= delta;
	}

	public RitualReward GetReward(ActiveRitual ritual)
	{
		if (RitualRules.IsStudyRitual(ritual.RitualId))
		{
			return new RitualReward
			{
				LearnedRitualId = ritual.BookRitualId,
				CoreLevelUp = ritual.IsCoreHosted
			};
		}

		RitualDef def = RitualRules.Get(ritual.RitualId);
		if (def == null)
			return new RitualReward();
		return ritual.IsCoreHosted ? def.CoreReward : def.BasicReward;
	}

	public RitualReward Complete(ActiveRitual ritual)
	{
		RitualReward reward = GetReward(ritual);
		if (string.IsNullOrEmpty(reward.LearnedRitualId))
			return reward;

		Learn(reward.LearnedRitualId);
		StudyCompletionCount++;
		return reward;
	}

	public bool IsBookStudiable(CardData book, int coreLevel)
	{
		if (book == null)
			return false;
		string ritualId = DarkMagicBookRules.GetRitualId(book);
		if (ritualId == null || IsLearned(ritualId))
			return false;
		RitualDef def = RitualRules.Get(ritualId);
		return def != null && def.RequiredCoreLevel <= coreLevel;
	}

	public bool IsDarkAltarUnlocked => IsLearned(RitualRules.EvilRitualId);

	public bool Learn(string ritualId)
	{
		if (ritualId == null || IsLearned(ritualId))
			return false;

		_learnedRituals.Add(ritualId);
		return true;
	}

	public bool IsLearned(string ritualId)
	{
		return ritualId != null && _learnedRituals.Contains(ritualId);
	}

	public IReadOnlyList<RitualDef> GetAvailableRituals()
	{
		List<RitualDef> available = new();
		foreach (RitualDef def in RitualRules.GetAll())
		{
			if (RitualRules.IsStudyRitual(def.Id) || IsLearned(def.Id))
				available.Add(def);
		}
		return available;
	}

	public IReadOnlyList<string> GetLearnedRitualIds()
	{
		List<string> ids = new();
		foreach (string ritualId in _learnedRituals)
			ids.Add(ritualId);
		return ids;
	}

	public void Restore(int studyCompletionCount, IReadOnlyList<string> learnedRitualIds)
	{
		Reset();
		StudyCompletionCount = studyCompletionCount;
		if (learnedRitualIds == null)
			return;
		foreach (string ritualId in learnedRitualIds)
			_learnedRituals.Add(ritualId);
	}

	public void Reset()
	{
		_learnedRituals.Clear();
		StudyCompletionCount = 0;
	}
}