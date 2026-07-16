using CasualCastle.Domain.Building;
using CasualCastle.Domain.History;
using CasualCastle.Domain.Shared;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class MirrorRulesTests
{
	[Fact]
	public void MirrorAnchor_SingleCell_ReturnsMirroredX()
	{
		GridCellOffset[] footprint = new[] { new GridCellOffset(0, 0) };
		(int x, int y) = MirrorRules.MirrorAnchor(3, 5, footprint, 10);

		Assert.Equal(6, x);
		Assert.Equal(5, y);
	}

	[Fact]
	public void MirrorAnchor_MultiCellWidth_AccountsForMaxOffset()
	{
		GridCellOffset[] footprint = new[] { new GridCellOffset(0, 0), new GridCellOffset(2, 0) };
		(int x, int y) = MirrorRules.MirrorAnchor(2, 5, footprint, 10);

		Assert.Equal(5, x);
		Assert.Equal(5, y);
	}

	[Fact]
	public void GetOccupiedCells_SingleCell_ReturnsOneCell()
	{
		GridCellOffset[] footprint = new[] { new GridCellOffset(0, 0) };
		System.Collections.Generic.List<(int x, int y)> cells = MirrorRules.GetOccupiedCells(3, 5, footprint);

		Assert.Single(cells);
		Assert.Equal((3, 5), cells[0]);
	}

	[Fact]
	public void GetOccupiedCells_MultiCell_ReturnsAllCells()
	{
		GridCellOffset[] footprint = new[] { new GridCellOffset(0, 0), new GridCellOffset(1, 0), new GridCellOffset(0, 1) };
		System.Collections.Generic.List<(int x, int y)> cells = MirrorRules.GetOccupiedCells(3, 5, footprint);

		Assert.Equal(3, cells.Count);
		Assert.Contains((3, 5), cells);
		Assert.Contains((4, 5), cells);
		Assert.Contains((3, 6), cells);
	}
}
