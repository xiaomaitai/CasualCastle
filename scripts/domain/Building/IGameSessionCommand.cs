// 对局会话命令端口：恢复与合并后刷新所需的运行时操作
using System.Collections.Generic;

namespace CasualCastle.Domain.Building;

public interface IGameSessionCommand
{
	void ClearNonCoreBuildings();
	void PlaceRestoredBuilding(string typeId, int anchorGridX, int anchorGridY, int health);
	bool RestoreCoreBuilding(string typeId, int health);
	void SetGold(int gold);
	void ResetHand();
	void AddRestoredCard(CardData card);
	void SetNightIndex(int index);
	void SetPendingReplayReportId(string reportId);
	void RestoreRitualState(int studyCompletionCount, IReadOnlyList<string> learnedRitualIds);
	void RestoreKingXp(int cumulativeExperience);
	void RestoreKingSkills(IReadOnlyList<string> skillIds, IReadOnlyList<int> levels);
	void RefreshPlayerAdjacency();
	void RefreshAfterCombine();
	void RefreshEnemyAdjacency();
}
