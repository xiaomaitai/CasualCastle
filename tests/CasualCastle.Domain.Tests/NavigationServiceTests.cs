using System;
using CasualCastle.Domain.Battle;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class NavigationServiceTests
{
	[Fact]
	public void Step_UnitWithGoal_MovesTowardGoal()
	{
		(NavigationService service, FakeUnitRepository repo, _) = CreateContext();
		Unit unit = CreateMarchingUnit(repo, 100f, 100f, 500f, 100f);

		service.Step(0.1f);

		Assert.True(unit.GameX > 100f);
		Assert.InRange(unit.GameY, 95f, 105f);
	}

	[Fact]
	public void Step_UnitWithoutGoal_StandsStill()
	{
		(NavigationService service, FakeUnitRepository repo, _) = CreateContext();
		Unit unit = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		unit.SetPosition(100f, 100f);
		repo.Register(unit);

		service.Step(0.1f);

		Assert.Equal(0f, unit.NavVelX);
		Assert.Equal(0f, unit.NavVelY);
		Assert.Equal(100f, unit.GameX);
		Assert.Equal(100f, unit.GameY);
	}

	[Fact]
	public void Step_UnitAtGoal_Stops()
	{
		(NavigationService service, FakeUnitRepository repo, _) = CreateContext();
		Unit unit = CreateMarchingUnit(repo, 100f, 100f, 100f, 100f);

		service.Step(0.1f);

		Assert.Equal(0f, unit.NavVelX);
		Assert.Equal(0f, unit.NavVelY);
	}

	[Fact]
	public void Step_FullWallBlocksUnit_UnitStops()
	{
		(NavigationService service, FakeUnitRepository repo, NavGrid grid) = CreateContext();
		for (int y = 0; y < 20; y++)
			grid.SetBlocked(6, y, true);
		Unit unit = CreateMarchingUnit(repo, 295f, 275f, 500f, 275f);

		service.Step(0.1f);

		Assert.Equal(295f, unit.GameX, 2);
		Assert.Equal(275f, unit.GameY, 2);
	}

	[Fact]
	public void Step_OverlappingUnits_AreSeparated()
	{
		(NavigationService service, FakeUnitRepository repo, _) = CreateContext();
		Unit a = CreateMarchingUnit(repo, 100f, 100f, 500f, 100f);
		Unit b = CreateMarchingUnit(repo, 100f, 108f, 500f, 100f);
		a.CollisionRadius = 20f;
		b.CollisionRadius = 20f;

		for (int i = 0; i < 20; i++)
			service.Step(0.05f);

		Assert.True(Math.Abs(a.GameY - b.GameY) > 20f);
	}

	[Fact]
	public void Step_DeeplyOverlappingUnits_ResolveToNoOverlap()
	{
		(NavigationService service, FakeUnitRepository repo, _) = CreateContext();
		Unit a = CreateMarchingUnit(repo, 100f, 100f, 500f, 100f);
		Unit b = CreateMarchingUnit(repo, 100f, 102f, 500f, 100f);
		a.CollisionRadius = 20f;
		b.CollisionRadius = 20f;

		for (int i = 0; i < 40; i++)
			service.Step(0.05f);

		float dist = MathF.Sqrt((a.GameX - b.GameX) * (a.GameX - b.GameX) + (a.GameY - b.GameY) * (a.GameY - b.GameY));
		Assert.True(dist >= 39f);
	}

	private static (NavigationService, FakeUnitRepository, NavGrid) CreateContext()
	{
		NavGrid grid = new(20, 20, 0f, 0f);
		FakeUnitRepository repo = new();
		NavigationService service = new(new FakeNavGridSource(grid), repo, new FlowFieldService(), new SteeringService());
		return (service, repo, grid);
	}

	private static Unit CreateMarchingUnit(FakeUnitRepository repo, float x, float y, float goalX, float goalY)
	{
		Unit unit = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		unit.CollisionRadius = 20f;
		unit.SetPosition(x, y);
		unit.UpdateTargeting(null);
		unit.UpdateBehavior(0.016f, goalX, goalY);
		repo.Register(unit);
		return unit;
	}
}

internal sealed class FakeNavGridSource : INavGridSource
{
	private readonly NavGrid _grid;

	public FakeNavGridSource(NavGrid grid)
	{
		_grid = grid;
	}

	public int Version => 0;

	public void Refresh()
	{
	}

	public NavGrid GetGrid(bool isPlayerUnit)
	{
		return _grid;
	}
}
