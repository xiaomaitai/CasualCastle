using CasualCastle.Domain.Battle;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class FlowFieldServiceTests
{
	private static NavGrid CreateOpenGrid()
	{
		return new NavGrid(20, 20, 0f, 0f);
	}

	[Fact]
	public void Compute_GoalCell_HasZeroDirection()
	{
		NavGrid grid = CreateOpenGrid();
		FlowFieldService service = new();
		int goal = grid.Index(10, 10);

		FlowField field = service.Compute(grid, goal);

		Assert.True(field.Reachable[goal]);
		Assert.Equal(0f, field.DirX[goal]);
		Assert.Equal(0f, field.DirY[goal]);
	}

	[Fact]
	public void Compute_NeighborOfGoal_PointsTowardGoal()
	{
		NavGrid grid = CreateOpenGrid();
		FlowFieldService service = new();
		int goal = grid.Index(10, 10);

		FlowField field = service.Compute(grid, goal);

		int left = grid.Index(9, 10);
		Assert.True(field.Reachable[left]);
		Assert.True(field.DirX[left] > 0f);
		Assert.Equal(0f, field.DirY[left]);
	}

	[Fact]
	public void Compute_BlockedCell_IsUnreachable()
	{
		NavGrid grid = CreateOpenGrid();
		grid.SetBlocked(5, 10, true);
		FlowFieldService service = new();
		int goal = grid.Index(10, 10);

		FlowField field = service.Compute(grid, goal);

		Assert.False(field.Reachable[grid.Index(5, 10)]);
	}

	[Fact]
	public void Compute_WallForcesDetour()
	{
		NavGrid grid = CreateOpenGrid();
		for (int y = 1; y <= 18; y++)
			grid.SetBlocked(5, y, true);
		FlowFieldService service = new();
		int goal = grid.Index(10, 10);

		FlowField field = service.Compute(grid, goal);

		Assert.True(field.Reachable[grid.Index(4, 0)]);
		Assert.True(field.Reachable[grid.Index(4, 19)]);
	}
}
