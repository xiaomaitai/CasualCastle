using System.Collections.Generic;

namespace CasualCastle.Domain.Battle;

public interface ICombatUseCase
{
	void PushUnits(IReadOnlyList<IUnitHandle> allUnits, IReadOnlyList<IBuildingRef> buildings, float dt);
}
