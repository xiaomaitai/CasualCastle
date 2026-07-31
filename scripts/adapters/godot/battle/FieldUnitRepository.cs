using System.Collections.Generic;
using CasualCastle.Domain.Battle;

namespace CasualCastle.Adapters.Godot.Battle;

public class FieldUnitRepository : IFieldUnitRepository
{
	private readonly List<IUnitHandle> _playerUnits = new();
	private readonly List<IUnitHandle> _enemyUnits = new();
	private readonly List<IBuildingRef> _buildings = new();

	public IReadOnlyList<IUnitHandle> AllUnits
	{
		get
		{
			List<IUnitHandle> all = new(_playerUnits.Count + _enemyUnits.Count);
			all.AddRange(_playerUnits);
			all.AddRange(_enemyUnits);
			return all;
		}
	}

	public IReadOnlyList<IBuildingRef> AllBuildings => _buildings;

	public void Register(IUnitHandle unit)
	{
		if (unit.IsPlayerUnit)
			_playerUnits.Add(unit);
		else
			_enemyUnits.Add(unit);
	}

	public void Unregister(IUnitHandle unit)
	{
		if (unit.IsPlayerUnit)
			_playerUnits.Remove(unit);
		else
			_enemyUnits.Remove(unit);
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
