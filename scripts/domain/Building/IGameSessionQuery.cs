// 对局会话查询端口：保存与阶段副作用所需的运行时状态
using System.Collections.Generic;

namespace CasualCastle.Domain.Building;

public interface IGameSessionQuery
{
	string FactionId { get; }
	int Gold { get; }
	int CurrentNightIndex { get; }
	string PendingReplayReportId { get; }
	List<IBuildingState> PlayerBuildingStates { get; }
	IReadOnlyList<CardData> HandCards { get; }
	ICombineBuildingFactory CombineFactory { get; }
	int RitualStudyCompletionCount { get; }
	IReadOnlyList<string> LearnedRitualIds { get; }
	int DungeonKingXp { get; }
	IReadOnlyList<string> DungeonLearnedSkillIds { get; }
	IReadOnlyList<int> DungeonLearnedSkillLevels { get; }
}
