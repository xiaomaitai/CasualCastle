using CasualCastle.Domain.Coordinates;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class GameCoordinateRulesTests
{
	[Fact]
	public void GetBuildingFootprintSpawnPoint_SingleCell_UsesBottomLeftOfBlock()
	{
		var footprint = new[] { new GridCellOffset(0, 0) };
		GameVector2 spawn = GameCoordinateRules.GetBuildingFootprintSpawnPoint(footprint, 7, 4, 0);

		Assert.Equal(706, spawn.X);
		Assert.Equal(494, spawn.Y);
	}

	[Fact]
	public void GetBuildingFootprintSpawnPoint_MultiCell_UsesFootprintBottomLeft()
	{
		var footprint = new[]
		{
			new GridCellOffset(0, 0),
			new GridCellOffset(1, 0),
			new GridCellOffset(0, 1),
			new GridCellOffset(1, 2),
		};
		GameVector2 spawn = GameCoordinateRules.GetBuildingFootprintSpawnPoint(footprint, 3, 2, 0);

		Assert.Equal(306, spawn.X);
		Assert.Equal(494, spawn.Y);
	}

	[Fact]
	public void GetBuildingFootprintSpawnPoint_SpreadsSubsequentSpawns()
	{
		var footprint = new[] { new GridCellOffset(0, 0) };
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
		// CellCorner(2,3) = (200, 300), inset = (100-94)/2 = 3
		Assert.Equal(203, origin.X);
		Assert.Equal(303, origin.Y);
	}
}
