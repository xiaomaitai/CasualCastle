using System.Collections.Generic;

namespace CasualCastle.Domain.Battle;

public interface ICombatUseCase
{
	void PushSoldiers(IReadOnlyList<ISoldierService> allUnits, float dt);
}
