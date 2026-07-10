using System.Collections.Generic;

namespace CasualCastle.Domain.Building;

public interface ICombineUseCase
{
	void ResolveCombines(List<IBuildingState> buildings, bool isPlayerSide, bool isNight, bool isPlaying);
}
