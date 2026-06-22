using Godot;
using System.Collections.Generic;

public partial class AdjacentSystem : Node
{
    public static AdjacentSystem Instance { get; private set; }

    private static readonly Vector2I[] Directions =
    {
        Vector2I.Up,
        Vector2I.Down,
        Vector2I.Left,
        Vector2I.Right,
    };

    public override void _Ready()
    {
        Instance = this;
    }

    public override void _ExitTree()
    {
        if (Instance == this)
            Instance = null;
    }

    public void RefreshCastle(Castle castle)
    {
        if (castle == null)
            return;

        List<Building> buildings = castle.GetBuildings();
        foreach (Building building in buildings)
            building.SetWorkSpeedMultiplier(1f);

        foreach (Building building in buildings)
            ApplyBonuses(building, buildings);
    }

    private static void ApplyBonuses(Building building, List<Building> buildings)
    {
        if (building.TypeId != "Barracks")
            return;

        int adjacentBarracks = CountAdjacentBuildings(building, buildings, "Barracks");
        if (adjacentBarracks > 0)
            building.SetWorkSpeedMultiplier(1f + 0.2f * adjacentBarracks);
    }

    private static int CountAdjacentBuildings(Building source, List<Building> buildings, string targetTypeId)
    {
        HashSet<Building> neighbors = new();
        Dictionary<Vector2I, Building> cellOwners = BuildCellOwnerMap(buildings);

        foreach (Vector2I cell in GetOccupiedCells(source))
        {
            foreach (Vector2I direction in Directions)
            {
                if (!cellOwners.TryGetValue(cell + direction, out Building neighbor))
                    continue;

                if (neighbor == source || neighbor.TypeId != targetTypeId)
                    continue;

                neighbors.Add(neighbor);
            }
        }

        return neighbors.Count;
    }

    private static Dictionary<Vector2I, Building> BuildCellOwnerMap(List<Building> buildings)
    {
        Dictionary<Vector2I, Building> cellOwners = new();

        foreach (Building building in buildings)
        {
            foreach (Vector2I cell in GetOccupiedCells(building))
                cellOwners[cell] = building;
        }

        return cellOwners;
    }

    private static IEnumerable<Vector2I> GetOccupiedCells(Building building)
    {
        foreach (Vector2I offset in BuildingSystem.GetFootprint(building.TypeId))
            yield return new Vector2I(building.AnchorGridX + offset.X, building.AnchorGridY + offset.Y);
    }
}
