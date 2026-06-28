using CasualCastle.Domain.Shared;
using System.Collections.Generic;

namespace CasualCastle.Domain.History;

public static class MirrorRules
{
    public static (int gridX, int gridY) MirrorAnchor(
        int anchorGridX, int anchorGridY,
        IReadOnlyList<GridCellOffset> footprint,
        int enemyGridColumns)
    {
        int maxOffsetX = 0;
        foreach (GridCellOffset offset in footprint)
            maxOffsetX = System.Math.Max(maxOffsetX, offset.X);

        int mirrorX = enemyGridColumns - 1 - anchorGridX - maxOffsetX;
        return (mirrorX, anchorGridY);
    }

    public static List<(int x, int y)> GetOccupiedCells(
        int anchorX, int anchorY, IReadOnlyList<GridCellOffset> footprint)
    {
        List<(int x, int y)> cells = new(footprint.Count);
        foreach (GridCellOffset offset in footprint)
            cells.Add((anchorX + offset.X, anchorY + offset.Y));
        return cells;
    }
}
