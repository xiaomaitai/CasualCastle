// 阵营定义与标识常量
using CasualCastle.Domain.Shared;

namespace CasualCastle.Domain.Building;

public class FactionDef
{
	public const string HordeId = FactionIds.Horde;
	public const string NecromancerId = FactionIds.Necromancer;
	public const string DungeonId = FactionIds.Dungeon;

	public string Id { get; init; }
	public string DisplayName { get; init; }
	public int SortOrder { get; init; }
}
