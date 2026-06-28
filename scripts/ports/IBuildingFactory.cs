using CasualCastle.Domain.Building;

namespace CasualCastle.Ports;

public interface IBuildingFactory
{
	Building Create(string typeId);
	void Destroy(Building building);
}
