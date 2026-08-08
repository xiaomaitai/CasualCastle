// 建筑行动规则：判定可施法或举行仪式的建筑
namespace CasualCastle.Domain.Building;

public static class BuildingActionRules
{
	public static bool CanUseSpellBar(string typeId)
	{
		return RitualRules.CanHostRitual(typeId);
	}
}
