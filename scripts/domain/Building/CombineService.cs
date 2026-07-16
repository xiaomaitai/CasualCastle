using System;
using System.Collections.Generic;
using System.Linq;

namespace CasualCastle.Domain.Building;

public class CombineService : ICombineUseCase
{
	private readonly IBuildingRepository _buildingRepo;
	private readonly CombineRules _combineRules;

	public CombineService(IBuildingRepository buildingRepo, CombineRules combineRules)
	{
		_buildingRepo = buildingRepo;
		_combineRules = combineRules;
	}

	public void ResolveCombines(List<IBuildingState> buildings, bool isPlayerSide, bool isNight, bool isPlaying, ICombineBuildingFactory factory, Action<IBuildingState> onCombineCompleted)
	{
		if (!isPlayerSide || !isPlaying || !isNight)
			return;

		HashSet<IBuildingState> used = new();

		while (true)
		{
			CombineGroup group = _combineRules.FindBestCombinableGroup(buildings, used, _buildingRepo);
			if (group == null)
				break;

			IBuildingState result = TryCombineGroup(buildings, group, factory);
			if (result == null)
			{
				used.Add(group.Main);
				foreach (IBuildingState mat in group.Materials)
					used.Add(mat);
			}
			else
			{
				onCombineCompleted?.Invoke(result);
			}
		}
	}

	private IBuildingState TryCombineGroup(List<IBuildingState> buildings, CombineGroup group, ICombineBuildingFactory factory)
	{
		if (!_combineRules.CanCombineGroup(group.Main, group.Materials, group.Recipe, _buildingRepo))
			return null;

		foreach (IBuildingState mat in group.Materials)
			factory.Destroy(mat);

		IBuildingState oldMain = group.Main;
		factory.Destroy(oldMain);

		IBuildingState result = factory.Create(
			group.Recipe.ResultTypeId,
			oldMain.AnchorGridX,
			oldMain.AnchorGridY);

		if (result == null)
			return null;

		buildings.RemoveAll(b => group.Materials.Contains(b) || b == oldMain);
		buildings.Add(result);

		return result;
	}
}
