namespace CasualCastle.Domain.Building;

public class FactionDef
{
	public const string HordeId = "horde";
	public const string NecromancerId = "necromancer";

	public string Id { get; init; }
	public string DisplayName { get; init; }
	public int SortOrder { get; init; }
}
