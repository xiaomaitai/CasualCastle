using Godot;
using System.Collections.Generic;

public partial class CastleHighlightOverlay : Node2D
{
    private Castle _castle;
    private readonly HashSet<Building> _buildings = new();

    public void Bind(Castle castle)
    {
        _castle = castle;
    }

    public void SetBuildings(IEnumerable<Building> buildings)
    {
        _buildings.Clear();
        if (buildings != null)
        {
            foreach (Building building in buildings)
                _buildings.Add(building);
        }

        QueueRedraw();
    }

    public void ClearHighlights()
    {
        if (_buildings.Count == 0)
            return;

        _buildings.Clear();
        QueueRedraw();
    }

    public override void _Draw()
    {
        if (_castle == null || _buildings.Count == 0)
            return;

        int cellSize = _castle.CellSize;
        Color highlightColor = new Color(0.45f, 0.78f, 1f, 0.95f);
        const float borderWidth = 2.5f;

        foreach (Building building in _buildings)
        {
            foreach (Vector2I offset in BuildingSystem.GetFootprint(building.TypeId))
            {
                int col = building.AnchorGridX + offset.X;
                int row = building.AnchorGridY + offset.Y;
                if (!_castle.IsInBounds(col, row))
                    continue;

                Vector2 cellPos = new Vector2(col * cellSize, row * cellSize);
                DrawRect(new Rect2(cellPos, new Vector2(cellSize, cellSize)), highlightColor, false, borderWidth);
            }
        }
    }
}
