using System.Collections.Generic;

namespace CasualCastle.Domain.Battle;

public class GameContext
{
	public float CurrentHpRatio;
	public HashSet<string> NearbyAllyRaces;
	public bool TargetIsIsolated;
}
