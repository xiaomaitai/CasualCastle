using System.Collections.Generic;
using CasualCastle.Domain.Battle;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class UnitSpatialServiceTests
{
	[Fact]
	public void PushUnits_UnitInsideEnemyBuilding_IsPushedOut()
	{
		Unit unit = UnitTestFactory.CreateUnit(AttackType.Melee, false);
		unit.SetPosition(50, 2);
		FakeBuilding building = new()
		{
			IsEnemyOfUnit = true,
			MinX = 0,
			MinY = 0,
			MaxX = 100,
			MaxY = 100,
		};

		Push(unit, building);

		Assert.Equal(50f, unit.GameX);
		Assert.Equal(-22f, unit.GameY);
	}

	[Fact]
	public void PushUnits_UnitOverlappingEnemyBuildingEdge_IsPushedOut()
	{
		Unit unit = UnitTestFactory.CreateUnit(AttackType.Melee, false);
		unit.SetPosition(50, -10);
		FakeBuilding building = new()
		{
			IsEnemyOfUnit = true,
			MinX = 0,
			MinY = 0,
			MaxX = 100,
			MaxY = 100,
		};

		Push(unit, building);

		Assert.Equal(50f, unit.GameX);
		Assert.Equal(-22f, unit.GameY);
	}

	[Fact]
	public void PushUnits_UnitInsideCoreBuilding_IsNotPushed()
	{
		Unit unit = UnitTestFactory.CreateUnit(AttackType.Melee, false);
		unit.SetPosition(50, 50);
		FakeBuilding building = new()
		{
			IsCore = true,
			IsEnemyOfUnit = true,
			MinX = 0,
			MinY = 0,
			MaxX = 100,
			MaxY = 100,
		};

		Push(unit, building);

		Assert.Equal(50f, unit.GameX);
		Assert.Equal(50f, unit.GameY);
	}

	[Fact]
	public void PushUnits_UnitInsideFriendlyBuilding_IsNotPushed()
	{
		Unit unit = UnitTestFactory.CreateUnit(AttackType.Melee, false);
		unit.SetPosition(50, 50);
		FakeBuilding building = new()
		{
			IsEnemyOfUnit = false,
			MinX = 0,
			MinY = 0,
			MaxX = 100,
			MaxY = 100,
		};

		Push(unit, building);

		Assert.Equal(50f, unit.GameX);
		Assert.Equal(50f, unit.GameY);
	}

	[Fact]
	public void PushUnits_UnitInsideDestroyedBuilding_IsNotPushed()
	{
		Unit unit = UnitTestFactory.CreateUnit(AttackType.Melee, false);
		unit.SetPosition(50, 50);
		FakeBuilding building = new()
		{
			IsDestroyed = true,
			IsEnemyOfUnit = true,
			MinX = 0,
			MinY = 0,
			MaxX = 100,
			MaxY = 100,
		};

		Push(unit, building);

		Assert.Equal(50f, unit.GameX);
		Assert.Equal(50f, unit.GameY);
	}

	[Fact]
	public void PushUnits_OverlappingAllies_ArePushedApart()
	{
		Unit a = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		Unit b = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		a.SetPosition(0, 0);
		b.SetPosition(2, 0);

		new UnitSpatialService().PushUnits(
			new List<IUnitHandle> { a, b },
			new List<IBuildingRef>(),
			0.016f);

		float dx = a.GameX - b.GameX;
		Assert.True(dx * dx > 4f);
	}

	[Fact]
	public void PushUnits_FightingUnit_IsNotPushedByAlly()
	{
		Unit fighter = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		Unit marcher = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		fighter.State = UnitState.Fighting;
		fighter.SetPosition(0, 0);
		marcher.SetPosition(2, 0);

		new UnitSpatialService().PushUnits(
			new List<IUnitHandle> { fighter, marcher },
			new List<IBuildingRef>(),
			0.016f);

		Assert.Equal(0f, fighter.GameX);
		Assert.Equal(2f, marcher.GameX);
	}

	[Fact]
	public void PushUnits_FightingUnit_IsNotPushedByBuilding()
	{
		Unit fighter = UnitTestFactory.CreateUnit(AttackType.Melee, false);
		fighter.State = UnitState.Fighting;
		fighter.SetPosition(50, 50);
		FakeBuilding building = new()
		{
			IsEnemyOfUnit = true,
			MinX = 0,
			MinY = 0,
			MaxX = 100,
			MaxY = 100,
		};

		Push(fighter, building);

		Assert.Equal(50f, fighter.GameX);
		Assert.Equal(50f, fighter.GameY);
	}

	private static void Push(Unit unit, IBuildingRef building)
	{
		new UnitSpatialService().PushUnits(
			new List<IUnitHandle> { unit },
			new List<IBuildingRef> { building },
			0.016f);
	}
}
