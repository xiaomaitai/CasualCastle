using CasualCastle.Domain.History;
using CasualCastle.Domain.Shared;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class PhaseMappingTests
{
	[Fact]
	public void ToBattlePhase_Combat_MapsToDay()
	{
		Assert.Equal(BattlePhase.Day, PhaseMapping.ToBattlePhase(MatchPhase.Combat));
	}

	[Fact]
	public void ToBattlePhase_Shop_MapsToNight()
	{
		Assert.Equal(BattlePhase.Night, PhaseMapping.ToBattlePhase(MatchPhase.Shop));
	}
}
