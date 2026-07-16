using CasualCastle.Domain.Building;
using CasualCastle.Domain.Shared;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class OccupancyGridTests
{
	[Fact]
	public void IsInBounds_WithinBounds_ReturnsTrue()
	{
		OccupancyGrid grid = new(10, 8);
		Assert.True(grid.IsInBounds(0, 0));
		Assert.True(grid.IsInBounds(9, 7));
	}

	[Fact]
	public void IsInBounds_OutOfBounds_ReturnsFalse()
	{
		OccupancyGrid grid = new(10, 8);
		Assert.False(grid.IsInBounds(-1, 0));
		Assert.False(grid.IsInBounds(0, -1));
		Assert.False(grid.IsInBounds(10, 0));
		Assert.False(grid.IsInBounds(0, 8));
	}

	[Fact]
	public void IsCellPassable_EmptyCell_ReturnsTrue()
	{
		OccupancyGrid grid = new(10, 8);
		Assert.True(grid.IsCellPassable(5, 5));
	}

	[Fact]
	public void IsCellPassable_OutOfBounds_ReturnsFalse()
	{
		OccupancyGrid grid = new(10, 8);
		Assert.False(grid.IsCellPassable(10, 8));
	}

	[Fact]
	public void OccupyCells_MarksCellsAsOccupied()
	{
		OccupancyGrid grid = new(10, 8);
		GridCellOffset[] footprint = new[] { new GridCellOffset(0, 0), new GridCellOffset(1, 0) };
		grid.OccupyCells(footprint, 3, 4);
		Assert.False(grid.IsCellPassable(3, 4));
		Assert.False(grid.IsCellPassable(4, 4));
	}

	[Fact]
	public void ReleaseCells_FreesOccupiedCells()
	{
		OccupancyGrid grid = new(10, 8);
		GridCellOffset[] footprint = new[] { new GridCellOffset(0, 0) };
		grid.OccupyCells(footprint, 3, 4);
		grid.ReleaseCells(footprint, 3, 4);
		Assert.True(grid.IsCellPassable(3, 4));
	}

	[Fact]
	public void CanPlaceFootprint_EmptyArea_ReturnsTrue()
	{
		OccupancyGrid grid = new(10, 8);
		GridCellOffset[] footprint = new[] { new GridCellOffset(0, 0), new GridCellOffset(1, 0) };
		Assert.True(grid.CanPlaceFootprint(footprint, 3, 4));
	}

	[Fact]
	public void CanPlaceFootprint_OccupiedArea_ReturnsFalse()
	{
		OccupancyGrid grid = new(10, 8);
		GridCellOffset[] footprint = new[] { new GridCellOffset(0, 0) };
		grid.OccupyCells(footprint, 3, 4);
		Assert.False(grid.CanPlaceFootprint(footprint, 3, 4));
	}

	[Fact]
	public void CanPlaceFootprint_OutOfBounds_ReturnsFalse()
	{
		OccupancyGrid grid = new(10, 8);
		GridCellOffset[] footprint = new[] { new GridCellOffset(0, 0) };
		Assert.False(grid.CanPlaceFootprint(footprint, 10, 8));
	}
}
