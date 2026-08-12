using CasualCastle.Domain.Battle;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class KingExperienceServiceV05Tests
{
	[Fact]
	public void OnKingKill_AppliesKillXpMultiplier()
	{
		KingExperienceService xp = new();
		xp.KillXpMultiplier = 1.5f;

		xp.OnKingKill();

		Assert.Equal((int)(DemonKingRules.KingKillXp * 1.5f), xp.Experience);
	}

	[Fact]
	public void Reset_RestoresKillXpMultiplier()
	{
		KingExperienceService xp = new();
		xp.KillXpMultiplier = 2f;

		xp.Reset();

		Assert.Equal(1f, xp.KillXpMultiplier);
	}
}