using CasualCastle.Domain.Building;

namespace CasualCastle.Adapters.Godot;

public interface IBuildingFactory
{
	Building Create(string typeId);
	void Destroy(Building building);
}
