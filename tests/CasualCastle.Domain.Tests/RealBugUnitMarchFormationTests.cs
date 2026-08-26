using System;
using System.Collections.Generic;
using CasualCastle.Domain.Battle;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class RealBugUnitMarchFormationTests
{
	private const float StepDt = 1f / 60f;

	private static NavGrid CreateOpenGrid()
	{
		return new NavGrid(40, 40, 0f, 0f);
	}

	private static FlowField CreateField(NavGrid grid, float goalX, float goalY)
	{
		FlowFieldService service = new FlowFieldService();
		int goal = grid.Index(grid.WorldToCellX(goalX), grid.WorldToCellY(goalY));
		return service.Compute(grid, goal);
	}

	private static Unit CreateMarchingUnit(float x, float y, float goalX, float goalY)
	{
		Unit unit = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		unit.SetPosition(x, y);
		unit.UpdateTargeting(null);
		unit.UpdateBehavior(0.016f, goalX, goalY);
		return unit;
	}

	[Fact]
	public void RealBug_RearUnitFlanksAroundAlignedStoppedFrontInsteadOfJamming()
	{
		NavGrid grid = CreateOpenGrid();
		FlowField field = CreateField(grid, 1300f, 500f);
		Unit blocker = CreateMarchingUnit(800f, 500f, 800f, 500f);
		Unit rear = CreateMarchingUnit(750f, 500f, 1300f, 500f);
		List<IUnitHandle> neighbors = new List<IUnitHandle> { blocker };
		SteeringService steering = new SteeringService();
		float maxLateralDeviation = 0f;

		for (int i = 0; i < 500; i++)
		{
			steering.Steer(rear, grid, field, neighbors, StepDt);
			maxLateralDeviation = Math.Max(maxLateralDeviation, Math.Abs(rear.GameY - 500f));
		}

		Assert.True(maxLateralDeviation > 20f);
		Assert.True(rear.GameX > 850f);
	}

	[Fact]
	public void RealBug_AlignedColumnSpreadsLateralInsteadOfSingleFile()
	{
		NavGrid grid = CreateOpenGrid();
		FlowField field = CreateField(grid, 1300f, 500f);
		Unit front = CreateMarchingUnit(850f, 500f, 1300f, 500f);
		Unit middle = CreateMarchingUnit(800f, 500f, 1300f, 500f);
		Unit back = CreateMarchingUnit(750f, 500f, 1300f, 500f);
		List<IUnitHandle> units = new List<IUnitHandle> { front, middle, back };
		SteeringService steering = new SteeringService();
		float maxSpread = 0f;

		for (int i = 0; i < 420; i++)
		{
			foreach (Unit unit in units)
				steering.Steer(unit, grid, field, units, StepDt);
			float minY = Math.Min(front.GameY, Math.Min(middle.GameY, back.GameY));
			float maxY = Math.Max(front.GameY, Math.Max(middle.GameY, back.GameY));
			maxSpread = Math.Max(maxSpread, maxY - minY);
		}

		Assert.True(maxSpread > 25f);
	}

	[Fact]
	public void RealBug_FarUnitsMarchIndependentlyWithoutLateralPull()
	{
		NavGrid grid = CreateOpenGrid();
		FlowField field = CreateField(grid, 1300f, 500f);
		Unit other = CreateMarchingUnit(550f, 500f, 1300f, 500f);
		Unit withNeighbor = CreateMarchingUnit(750f, 500f, 1300f, 500f);
		Unit withoutNeighbor = CreateMarchingUnit(750f, 500f, 1300f, 500f);
		List<IUnitHandle> neighbors = new List<IUnitHandle> { other };
		List<IUnitHandle> none = new List<IUnitHandle>();
		SteeringService steering = new SteeringService();

		for (int i = 0; i < 90; i++)
		{
			steering.Steer(withNeighbor, grid, field, neighbors, StepDt);
			steering.Steer(withoutNeighbor, grid, field, none, StepDt);
		}

		Assert.Equal(withoutNeighbor.GameX, withNeighbor.GameX, 2);
		Assert.Equal(withoutNeighbor.GameY, withNeighbor.GameY, 2);
		Assert.True(withNeighbor.GameX > 800f);
	}

	[Fact]
	public void SingleUnit_ReachingGoal_StopsWithinArrivalRadius()
	{
		NavGrid grid = CreateOpenGrid();
		FlowField field = CreateField(grid, 760f, 500f);
		Unit unit = CreateMarchingUnit(750f, 500f, 760f, 500f);
		List<IUnitHandle> neighbors = new List<IUnitHandle>();
		SteeringService steering = new SteeringService();

		for (int i = 0; i < 120; i++)
			steering.Steer(unit, grid, field, neighbors, StepDt);

		float dx = unit.GameX - 760f;
		float dy = unit.GameY - 500f;
		Assert.True(Math.Sqrt(dx * dx + dy * dy) <= 8.05f);
		Assert.Equal(0f, unit.NavVelX);
		Assert.Equal(0f, unit.NavVelY);
	}
}