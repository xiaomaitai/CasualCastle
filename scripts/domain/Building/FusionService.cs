using System;
using System.Collections.Generic;
using System.Linq;

namespace CasualCastle.Domain.Building;

public class FusionService
{
    public event Action<IBuildingState> FusionCompleted;

    private readonly IFusionBuildingFactory _factory;

    public FusionService(IFusionBuildingFactory factory)
    {
        _factory = factory;
    }

    public void ResolveFusions(List<IBuildingState> buildings, bool isPlayerSide, bool isNight, bool isPlaying)
    {
        if (!isPlayerSide || !isPlaying || !isNight)
            return;

        HashSet<IBuildingState> used = new();

        while (true)
        {
            FusionGroup group = FusionRules.FindBestFusibleGroup(buildings, used);
            if (group == null)
                break;

            if (!TryFuseGroup(buildings, group))
            {
                used.Add(group.Main);
                foreach (var mat in group.Materials)
                    used.Add(mat);
            }
        }
    }

    private bool TryFuseGroup(List<IBuildingState> buildings, FusionGroup group)
    {
        if (!FusionRules.CanFuseGroup(group.Main, group.Materials, group.Recipe))
            return false;

        foreach (var mat in group.Materials)
            _factory.Destroy(mat);

        IBuildingState oldMain = group.Main;
        _factory.Destroy(oldMain);

        IBuildingState result = _factory.Create(
            group.Recipe.ResultTypeId,
            oldMain.AnchorGridX,
            oldMain.AnchorGridY);

        if (result == null)
            return false;

        buildings.RemoveAll(b => group.Materials.Contains(b) || b == oldMain);
        buildings.Add(result);

        FusionCompleted?.Invoke(result);
        return true;
    }
}
