using CasualCastle.Domain.Shared;
using System.Collections.Generic;

namespace CasualCastle.Domain.Building;

public class OccupancyGrid
{
    public int Columns { get; }
    public int Rows { get; }
    private readonly bool[,] _occupied;

    public OccupancyGrid(int columns, int rows)
    {
        Columns = columns;
        Rows = rows;
        _occupied = new bool[columns, rows];
    }

    public bool IsInBounds(int gridX, int gridY)
    {
        return gridX >= 0 && gridX < Columns && gridY >= 0 && gridY < Rows;
    }

    public bool IsCellPassable(int gridX, int gridY)
    {
        return IsInBounds(gridX, gridY) && !_occupied[gridX, gridY];
    }

    public bool CanPlaceFootprint(IReadOnlyList<GridCellOffset> footprint, int anchorX, int anchorY)
    {
        foreach (GridCellOffset offset in footprint)
        {
            if (!IsCellPassable(anchorX + offset.X, anchorY + offset.Y))
                return false;
        }
        return true;
    }

    public void OccupyCells(IReadOnlyList<GridCellOffset> footprint, int anchorX, int anchorY)
    {
        foreach (GridCellOffset offset in footprint)
            _occupied[anchorX + offset.X, anchorY + offset.Y] = true;
    }

    public void ReleaseCells(IReadOnlyList<GridCellOffset> footprint, int anchorX, int anchorY)
    {
        foreach (GridCellOffset offset in footprint)
        {
            int x = anchorX + offset.X;
            int y = anchorY + offset.Y;
            if (IsInBounds(x, y))
                _occupied[x, y] = false;
        }
    }
}
