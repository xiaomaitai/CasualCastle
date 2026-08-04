using System.Collections.Generic;
using CasualCastle.Domain.Battle;

namespace CasualCastle.Domain.Tests;

internal sealed class RecordingNavPort : INavigationPort
{
	public float LastTargetX { get; private set; }
	public float LastTargetY { get; private set; }

	public void SetTarget(float gameX, float gameY)
	{
		LastTargetX = gameX;
		LastTargetY = gameY;
	}

	public void ConfigureRvo(float radius, float neighborDistance, float timeHorizon)
	{
	}
}

internal sealed class FakeUnitRepository : IFieldUnitRepository
{
	private readonly List<IUnitHandle> _units = new();
	private readonly List<IBuildingRef> _buildings = new();

	public IReadOnlyList<IUnitHandle> AllUnits => _units;
	public IReadOnlyList<IBuildingRef> AllBuildings => _buildings;

	public void Register(IUnitHandle unit)
	{
		_units.Add(unit);
	}

	public void Unregister(IUnitHandle unit)
	{
		_units.Remove(unit);
	}

	public void RegisterBuilding(IBuildingRef building)
	{
		_buildings.Add(building);
	}

	public void UnregisterBuilding(IBuildingRef building)
	{
		_buildings.Remove(building);
	}
}

internal sealed class FakeBuilding : IBuildingRef
{
	public bool IsDestroyed { get; init; }
	public bool IsCore { get; init; }
	public bool IsEnemyOfUnit { get; init; }
	public float MinX { get; init; }
	public float MinY { get; init; }
	public float MaxX { get; init; }
	public float MaxY { get; init; }
	public IBuildingTarget BuildingTarget => null;

	public bool IsEnemyOf(IUnitState unit)
	{
		return IsEnemyOfUnit;
	}
}

internal static class UnitTestFactory
{
	public static Unit CreateUnit(AttackType attackType, bool isPlayerUnit, INavigationPort navPort = null)
	{
		return CreateUnit(attackType, isPlayerUnit, 90f, navPort);
	}

	public static Unit CreateUnit(AttackType attackType, bool isPlayerUnit, float attackRange, INavigationPort navPort = null)
	{
		return CreateUnit(attackType, isPlayerUnit, attackRange, navPort, false);
	}

	public static Unit CreateUnit(AttackType attackType, bool isPlayerUnit, float attackRange, INavigationPort navPort, bool canDetectStealth)
	{
		UnitStats stats = new()
		{
			TypeId = "test",
			Size = UnitSize.Small,
			AttackType = attackType,
			DamageType = DamageType.Slash,
			ArmorType = ArmorType.Leather,
			Health = 100,
			Damage = 20,
			Speed = 50,
			AttackRange = attackRange,
			AttackCooldown = 0.5f,
			VisionRange = 400,
			CanDetectStealth = canDetectStealth,
		};
		Unit unit = new(navPort ?? new RecordingNavPort());
		unit.Initialize(stats, isPlayerUnit);
		unit.SetDamageMatrix(new DamageMatrix());
		return unit;
	}

	public static SkillDef CreateStealthSkill(float engageDistance, float speedMult)
	{
		string distanceText = engageDistance.ToString("0.#", System.Globalization.CultureInfo.InvariantCulture);
		string speedText = speedMult.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);
		string json = "{\"behavior\":\"stealth\",\"param\":" + distanceText + ",\"speed_mult\":" + speedText + "}";
		return new SkillDef
		{
			Id = "stealth",
			DisplayName = "Stealth",
			Type = SkillType.Special,
			ConfigJson = json
		};
	}
}
