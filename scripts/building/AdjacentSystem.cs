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

    public void OnBuildingPlaced(Castle castle, Building placedBuilding)
    {
        RefreshCastle(castle);
        PlayAdjacencyPulses(castle, placedBuilding);
    }

    public void RefreshCastle(Castle castle)
    {
        if (castle == null)
            return;

        List<Building> buildings = castle.GetBuildings();
        foreach (Building building in buildings)
            ApplyBonuses(building, buildings);
    }

    private void PlayAdjacencyPulses(Castle castle, Building placedBuilding)
    {
        List<Building> buildings = castle.GetBuildings();
        foreach (Building neighbor in GetAdjacentBuildings(placedBuilding, buildings))
            SpawnPulse(castle, neighbor);
    }

    private static void SpawnPulse(Castle castle, Building building)
    {
        Vector2I mainGrid = building.GetMainGridPosition();
        Vector2 localPos = castle.GetCellCenter(mainGrid.X, mainGrid.Y);

        var pulse = new AdjacentLinkPulse();
        pulse.Configure(castle.CellSize);
        pulse.Position = localPos;
        castle.AddChild(pulse);
    }

    public IReadOnlyList<Building> GetAdjacencyEffectTargets(Building source)
    {
        if (source?.GetCastle() == null)
            return System.Array.Empty<Building>();

        return GetAdjacencyEffectTargets(source, source.GetCastle().GetBuildings());
    }

    private static List<Building> GetAdjacencyEffectTargets(Building source, List<Building> buildings)
    {
        List<Building> targets = new();

        if (source.TypeId == "Barracks" && source.ContributesToAdjacency)
        {
            foreach (Building neighbor in GetAdjacentBuildings(source, buildings))
            {
                if (neighbor.TypeId == "Barracks" && neighbor.ContributesToAdjacency)
                    targets.Add(neighbor);
            }
        }

        return targets;
    }

    private static void ApplyBonuses(Building building, List<Building> buildings)
    {
        float multiplier = 1f;
        if (building.ContributesToAdjacency && building.TypeId == "Barracks")
        {
            int adjacentBarracks = CountAdjacentBuildings(building, buildings, "Barracks");
            if (adjacentBarracks > 0)
                multiplier = 1f + 0.2f * adjacentBarracks;
        }

        building.SetWorkSpeedMultiplier(multiplier);
    }

    private static HashSet<Building> GetAdjacentBuildings(Building source, List<Building> buildings)
    {
        HashSet<Building> neighbors = new();
        Dictionary<Vector2I, Building> cellOwners = BuildCellOwnerMap(buildings);

        foreach (Vector2I cell in GetOccupiedCells(source))
        {
            foreach (Vector2I direction in Directions)
            {
                if (!cellOwners.TryGetValue(cell + direction, out Building neighbor))
                    continue;

                if (neighbor == source)
                    continue;

                neighbors.Add(neighbor);
            }
        }

        return neighbors;
    }

    private static int CountAdjacentBuildings(Building source, List<Building> buildings, string targetTypeId)
    {
        int count = 0;
        foreach (Building neighbor in GetAdjacentBuildings(source, buildings))
        {
            if (neighbor.TypeId == targetTypeId && neighbor.ContributesToAdjacency)
                count++;
        }

        return count;
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
