using System.Collections.Generic;
using CasualCastle.Domain.Battle;

namespace CasualCastle.Adapters.Godot.Battle;

public class FieldUnitRepository : IFieldUnitRepository
{
	private readonly List<ISoldierHandle> _playerUnits = new();
	private readonly List<ISoldierHandle> _enemyUnits = new();
	private readonly List<IBuildingRef> _buildings = new();

	public IReadOnlyList<ISoldierHandle> AllUnits
	{
		get
		{
			List<ISoldierHandle> all = new(_playerUnits.Count + _enemyUnits.Count);
			all.AddRange(_playerUnits);
			all.AddRange(_enemyUnits);
			return all;
		}
	}

	public IReadOnlyList<IBuildingRef> AllBuildings => _buildings;

	public void Register(ISoldierHandle soldier)
	{
		if (soldier.IsPlayerUnit)
			_playerUnits.Add(soldier);
		else
			_enemyUnits.Add(soldier);
	}

	public void Unregister(ISoldierHandle soldier)
	{
		if (soldier.IsPlayerUnit)
			_playerUnits.Remove(soldier);
		else
			_enemyUnits.Remove(soldier);
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
