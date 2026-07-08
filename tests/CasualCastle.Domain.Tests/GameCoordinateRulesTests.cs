using CasualCastle.Domain.Shared;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class GameCoordinateRulesTests
{
	[Fact]
	public void GetBuildingFootprintSpawnPoint_SingleCell_UsesBottomLeftOfBlock()
	{
		GridCellOffset[] footprint = new[] { new GridCellOffset(0, 0) };
		GameVector2 spawn = GameCoordinateRules.GetBuildingFootprintSpawnPoint(footprint, 7, 4, 0);

		Assert.Equal(710, spawn.X);
		Assert.Equal(490, spawn.Y);
	}

	[Fact]
	public void GetBuildingFootprintSpawnPoint_MultiCell_UsesFootprintBottomLeft()
	{
		GridCellOffset[] footprint = new[]
		{
			new GridCellOffset(0, 0),
			new GridCellOffset(1, 0),
			new GridCellOffset(0, 1),
			new GridCellOffset(1, 2),
		};
		GameVector2 spawn = GameCoordinateRules.GetBuildingFootprintSpawnPoint(footprint, 3, 2, 0);

		Assert.Equal(310, spawn.X);
		Assert.Equal(490, spawn.Y);
	}

	[Fact]
	public void GetBuildingFootprintSpawnPoint_SpreadsSubsequentSpawns()
	{
		GridCellOffset[] footprint = new[] { new GridCellOffset(0, 0) };
		GameVector2 first = GameCoordinateRules.GetBuildingFootprintSpawnPoint(footprint, 0, 0, 0);
		GameVector2 second = GameCoordinateRules.GetBuildingFootprintSpawnPoint(footprint, 0, 0, 1);

		Assert.Equal(first.X + GameCoordinateRules.UnitSpawnSpreadStepX, second.X);
		Assert.Equal(first.Y - GameCoordinateRules.UnitSpawnSpreadStepY, second.Y);
	}

	[Fact]
	public void CellCenter_IsMiddleOfCellInGameUnits()
	{
		GameVector2 center = GameCoordinateRules.CellCenter(2, 3);
		Assert.Equal(250, center.X);
		Assert.Equal(350, center.Y);
	}

	[Fact]
	public void CellCorner_IsTopLeftOfCellInGameUnits()
	{
		GameVector2 corner = GameCoordinateRules.CellCorner(3, 2);
		Assert.Equal(300, corner.X);
		Assert.Equal(200, corner.Y);
	}

	[Fact]
	public void CellBlockOrigin_IsCellCornerPlusInset()
	{
		GameVector2 origin = GameCoordinateRules.CellBlockOrigin(2, 3);
		Assert.Equal(207, origin.X);
		Assert.Equal(307, origin.Y);
	}

	[Fact]
	public void GetBuildingCollisionSize_SingleCell_ReturnsBlockSize()
	{
		GridCellOffset[] footprint = new[] { new GridCellOffset(0, 0) };
		GameVector2 size = GameCoordinateRules.GetBuildingCollisionSize(footprint);

		Assert.Equal(GameCoordinateRules.CellBlockSize, size.X);
		Assert.Equal(GameCoordinateRules.CellBlockSize, size.Y);
	}

	[Fact]
	public void GetBuildingCollisionSize_MultiCell_ScalesWithFootprint()
	{
		GridCellOffset[] footprint = new[]
		{
			new GridCellOffset(0, 0),
			new GridCellOffset(1, 0),
			new GridCellOffset(0, 1),
			new GridCellOffset(1, 1),
		};
		GameVector2 size = GameCoordinateRules.GetBuildingCollisionSize(footprint);

		Assert.Equal(2 * GameCoordinateRules.UnitsPerCell - GameCoordinateRules.CellGapSize, size.X);
		Assert.Equal(2 * GameCoordinateRules.UnitsPerCell - GameCoordinateRules.CellGapSize, size.Y);
	}

	[Fact]
	public void GetBuildingCollisionSize_IrregularFootprint_UsesBoundingBox()
	{
		GridCellOffset[] footprint = new[]
		{
			new GridCellOffset(0, 0),
			new GridCellOffset(0, 1),
			new GridCellOffset(0, 2),
			new GridCellOffset(1, 2),
		};
		GameVector2 size = GameCoordinateRules.GetBuildingCollisionSize(footprint);

		Assert.Equal(2 * GameCoordinateRules.UnitsPerCell - GameCoordinateRules.CellGapSize, size.X);
		Assert.Equal(3 * GameCoordinateRules.UnitsPerCell - GameCoordinateRules.CellGapSize, size.Y);
	}
}
