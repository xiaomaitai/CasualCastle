using System.Collections.Generic;

namespace CasualCastle.Domain.Building;

public interface IFusionUseCase
{
	void ResolveFusions(List<IBuildingState> buildings, bool isPlayerSide, bool isNight, bool isPlaying);
}
