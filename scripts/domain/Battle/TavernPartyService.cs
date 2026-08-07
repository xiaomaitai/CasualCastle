// 酒馆组队服务：冒险者集结、种族槽位、满员出发、弹出与释放
using System.Collections.Generic;

namespace CasualCastle.Domain.Battle;

public sealed class TavernPartyService
{
	public const string InnTypeId = "Inn";
	public const string AdventurerGuildTypeId = "AdventurerGuild";
	public const int InnPartyLimit = 4;
	public const int AdventurerGuildPartyLimit = 6;
	public const float StandRadius = 40f;

	private static readonly string[] AdventurerRaces = { "Elf", "Dwarf", "Halfling", "Dragonborn" };
	private readonly Dictionary<IUnitHandle, IBuildingRef> _assignments = new();
	private readonly Dictionary<IBuildingRef, List<IUnitHandle>> _waitingByTavern = new();

	public static bool IsTavernBuilding(string typeId)
	{
		return typeId == InnTypeId || typeId == AdventurerGuildTypeId;
	}

	public static bool IsAdventurer(IUnitState unit)
	{
		foreach (string race in AdventurerRaces)
		{
			if (unit.Race == race)
				return true;
		}
		return false;
	}

	public static int GetPartyLimit(string tavernTypeId)
	{
		return tavernTypeId == AdventurerGuildTypeId ? AdventurerGuildPartyLimit : InnPartyLimit;
	}

	public static (float X, float Y) GetTavernStandTarget(IUnitState unit, IBuildingRef tavern)
	{
		float targetX = Clamp(unit.GameX, tavern.MinX, tavern.MaxX);
		float targetY = Clamp(unit.GameY, tavern.MinY, tavern.MaxY);
		return (targetX, targetY);
	}

	public static bool IsNearTavern(IUnitState unit, IBuildingRef tavern)
	{
		(float targetX, float targetY) = GetTavernStandTarget(unit, tavern);
		float dx = unit.GameX - targetX;
		float dy = unit.GameY - targetY;
		float radius = StandRadius + unit.CollisionRadius;
		return dx * dx + dy * dy <= radius * radius;
	}

	public bool TryAssignTavern(IUnitHandle unit, IReadOnlyList<IBuildingRef> taverns)
	{
		if (!unit.IsAlive || !IsAdventurer(unit))
			return false;

		IBuildingRef tavern = FindNearestFriendlyTavern(unit, taverns);
		if (tavern == null)
			return false;

		if (ContainsRace(GetWaitingUnits(tavern), unit.Race))
			return false;

		_assignments[unit] = tavern;
		unit.AssignedTavern = tavern;
		Join(tavern, unit);
		TryDepart(tavern);
		return true;
	}

	public IReadOnlyList<IUnitHandle> GetWaitingUnits(IBuildingRef tavern)
	{
		if (_waitingByTavern.TryGetValue(tavern, out List<IUnitHandle> waiting))
			return waiting;
		return System.Array.Empty<IUnitHandle>();
	}

	public bool IsOccupied(IBuildingRef tavern)
	{
		return _waitingByTavern.TryGetValue(tavern, out List<IUnitHandle> waiting) && waiting.Count > 0;
	}

	public void Eject(IUnitHandle unit)
	{
		if (!_assignments.TryGetValue(unit, out IBuildingRef tavern))
			return;

		RemoveFromWaiting(tavern, unit);
		_assignments.Remove(unit);
		unit.AssignedTavern = null;
	}

	public void RemoveOnDeath(IUnitHandle unit)
	{
		if (!_assignments.TryGetValue(unit, out IBuildingRef tavern))
			return;

		RemoveFromWaiting(tavern, unit);
		_assignments.Remove(unit);
		unit.AssignedTavern = null;
	}

	public void ReleaseTavern(IBuildingRef tavern)
	{
		if (!_waitingByTavern.TryGetValue(tavern, out List<IUnitHandle> waiting))
			return;

		foreach (IUnitHandle unit in waiting)
		{
			_assignments.Remove(unit);
			unit.AssignedTavern = null;
		}
		waiting.Clear();
		_waitingByTavern.Remove(tavern);
	}

	private void Join(IBuildingRef tavern, IUnitHandle unit)
	{
		if (!_waitingByTavern.TryGetValue(tavern, out List<IUnitHandle> waiting))
		{
			waiting = new List<IUnitHandle>();
			_waitingByTavern[tavern] = waiting;
		}
		waiting.Add(unit);
	}

	private void TryDepart(IBuildingRef tavern)
	{
		if (!_waitingByTavern.TryGetValue(tavern, out List<IUnitHandle> waiting))
			return;

		int distinctRaces = CountDistinctRaces(waiting);
		if (distinctRaces < GetPartyLimit(tavern.TypeId))
			return;

		foreach (IUnitHandle unit in waiting)
		{
			_assignments.Remove(unit);
			unit.AssignedTavern = null;
		}
		waiting.Clear();
	}

	private static int CountDistinctRaces(List<IUnitHandle> waiting)
	{
		HashSet<string> races = new();
		foreach (IUnitHandle unit in waiting)
			races.Add(unit.Race);
		return races.Count;
	}

	private static bool ContainsRace(IReadOnlyList<IUnitHandle> waiting, string race)
	{
		foreach (IUnitHandle unit in waiting)
		{
			if (unit.Race == race)
				return true;
		}
		return false;
	}

	private static IBuildingRef FindNearestFriendlyTavern(IUnitHandle unit, IReadOnlyList<IBuildingRef> taverns)
	{
		IBuildingRef best = null;
		float bestDistanceSq = float.MaxValue;
		foreach (IBuildingRef tavern in taverns)
		{
			if (tavern.IsDestroyed || tavern.IsEnemyOf(unit))
				continue;

			(float targetX, float targetY) = GetTavernStandTarget(unit, tavern);
			float dx = unit.GameX - targetX;
			float dy = unit.GameY - targetY;
			float distanceSq = dx * dx + dy * dy;
			if (distanceSq >= bestDistanceSq)
				continue;

			best = tavern;
			bestDistanceSq = distanceSq;
		}
		return best;
	}

	private void RemoveFromWaiting(IBuildingRef tavern, IUnitHandle unit)
	{
		if (_waitingByTavern.TryGetValue(tavern, out List<IUnitHandle> waiting))
			waiting.Remove(unit);
	}

	private static float Clamp(float value, float min, float max)
	{
		if (value < min)
			return min;
		if (value > max)
			return max;
		return value;
	}
}