namespace CasualCastle.Domain.Shared;

public readonly struct GridCellOffset
{
	public int X { get; }
	public int Y { get; }

	public GridCellOffset(int x, int y)
	{
		X = x;
		Y = y;
	}
}
