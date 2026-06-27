namespace CasualCastle.Domain.Fusion;

public class FusionRecipe
{
	public string MainTypeId { get; init; }
	public string MaterialTypeId { get; init; }
	public int MaterialCount { get; init; }
	public string ResultTypeId { get; init; }
}
