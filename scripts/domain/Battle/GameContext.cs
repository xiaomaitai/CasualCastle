using System.Collections.Generic;

namespace CasualCastle.Domain.Battle;

public class GameContext
{
	public float CurrentHpRatio;
	public string Race;
	public bool IsFurious;
	public HashSet<string> NearbyAllyRaces;
	public bool TargetIsIsolated;
	public float AuraAttackMultiplier = 1f;
}
