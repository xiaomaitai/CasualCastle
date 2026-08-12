using CasualCastle.Domain.Battle;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class TreasureBoxTests
{
	[Fact]
	public void GetConfig_HasAttractFuseAndPull()
	{
		TrapConfig config = TrapRules.GetConfig(TrapRules.TreasureBox);

		Assert.Equal(60, config.Damage);
		Assert.True(config.AttractRange > 0f);
		Assert.True(config.FuseSeconds > 0f);
		Assert.True(config.PullSpeed > 0f);
	}

	[Fact]
	public void IsTrap_IncludesTreasureBox()
	{
		Assert.True(TrapRules.IsTrap(TrapRules.TreasureBox));
	}

	[Fact]
	public void PullTowards_MovesEnemyCloserToTarget()
	{
		Unit enemy = UnitTestFactory.CreateUnit(AttackType.Melee, false);
		enemy.SetPosition(100f, 0f);
		TrapService service = new(new FakeTacticalQueries(), new DamageMatrix());

		service.PullTowards(enemy, 0f, 0f, 50f, 1f);

		Assert.Equal(50f, enemy.GameX);
		Assert.Equal(0f, enemy.GameY);
	}

	[Fact]
	public void PullTowards_DoesNotOvershootTarget()
	{
		Unit enemy = UnitTestFactory.CreateUnit(AttackType.Melee, false);
		enemy.SetPosition(10f, 0f);
		TrapService service = new(new FakeTacticalQueries(), new DamageMatrix());

		service.PullTowards(enemy, 0f, 0f, 50f, 1f);

		Assert.Equal(0f, enemy.GameX);
	}
}