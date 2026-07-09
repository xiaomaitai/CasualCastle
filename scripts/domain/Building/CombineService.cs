using System;
using System.Collections.Generic;
using System.Linq;

namespace CasualCastle.Domain.Building;

public class CombineService : ICombineUseCase
{
	public event Action<IBuildingState> CombineCompleted;

	private readonly ICombineBuildingFactory _factory;
	private readonly IBuildingRepository _buildingRepo;

	public CombineService(ICombineBuildingFactory factory, IBuildingRepository buildingRepo)
	{
		_factory = factory;
		_buildingRepo = buildingRepo;
	}

	public void ResolveCombines(List<IBuildingState> buildings, bool isPlayerSide, bool isNight, bool isPlaying)
	{
		if (!isPlayerSide || !isPlaying || !isNight)
			return;

		HashSet<IBuildingState> used = new();

		while (true)
		{
			CombineGroup group = CombineRules.FindBestCombinableGroup(buildings, used, _buildingRepo);
			if (group == null)
				break;

			if (!TryCombineGroup(buildings, group))
			{
				used.Add(group.Main);
				foreach (IBuildingState mat in group.Materials)
					used.Add(mat);
			}
		}
	}

	private bool TryCombineGroup(List<IBuildingState> buildings, CombineGroup group)
	{
		if (!CombineRules.CanCombineGroup(group.Main, group.Materials, group.Recipe, _buildingRepo))
			return false;

		foreach (IBuildingState mat in group.Materials)
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

		CombineCompleted?.Invoke(result);
		return true;
	}
}
