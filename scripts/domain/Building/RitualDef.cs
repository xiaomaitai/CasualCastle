// 仪式定义与奖励数据
namespace CasualCastle.Domain.Building;

public enum RitualCostType
{
	Gold,
	BlackMagic
}

public class RitualReward
{
	public int GoldAmount;
	public string SummonUnitTypeId;
	public int SummonCount;
	public int UndeadMaxHealthBonus;
	public string LearnedRitualId;
	public bool CoreLevelUp;
}

public class RitualDef
{
	public string Id { get; init; }
	public string DisplayName { get; init; }
	public int RequiredCoreLevel { get; init; }
	public RitualCostType CostType { get; init; }
	public int CostAmount { get; init; }
	public float DurationSeconds { get; init; }
	public RitualReward BasicReward { get; init; }
	public RitualReward CoreReward { get; init; }
}