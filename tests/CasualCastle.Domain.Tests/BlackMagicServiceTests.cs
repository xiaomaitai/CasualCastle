using CasualCastle.Domain.Battle;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class BlackMagicServiceTests
{
	[Fact]
	public void OnUnitDied_FriendlyLiving_AddsFriendlyYield()
	{
		BlackMagicService service = new();

		service.OnUnitDied(true, "Human");

		Assert.Equal(BlackMagicService.DefaultFriendlyDeathYield, service.Value);
	}

	[Fact]
	public void OnUnitDied_EnemyLiving_AddsEnemyYield()
	{
		BlackMagicService service = new();

		service.OnUnitDied(false, "Orc");

		Assert.Equal(BlackMagicService.DefaultEnemyDeathYield, service.Value);
	}

	[Fact]
	public void OnUnitDied_NonLiving_AddsNothing()
	{
		BlackMagicService service = new();

		service.OnUnitDied(true, "Mechanical");
		service.OnUnitDied(false, "Elemental");
		service.OnUnitDied(true, "Golem");
		service.OnUnitDied(false, "Construct");

		Assert.Equal(0, service.Value);
	}

	[Fact]
	public void OnUnitDied_MixedDeaths_Accumulates()
	{
		BlackMagicService service = new();

		service.OnUnitDied(true, "Human");
		service.OnUnitDied(false, "Orc");
		service.OnUnitDied(true, "Undead");

		Assert.Equal(BlackMagicService.DefaultFriendlyDeathYield * 2 + BlackMagicService.DefaultEnemyDeathYield, service.Value);
	}

	[Fact]
	public void OnUnitDied_ReturnsGrantedYield()
	{
		BlackMagicService service = new();

		Assert.Equal(BlackMagicService.DefaultFriendlyDeathYield, service.OnUnitDied(true, "Human"));
		Assert.Equal(BlackMagicService.DefaultEnemyDeathYield, service.OnUnitDied(false, "Orc"));
		Assert.Equal(0, service.OnUnitDied(true, "Golem"));
	}

	[Fact]
	public void Add_NonPositiveAmount_ReturnsZero()
	{
		BlackMagicService service = new();

		Assert.Equal(0, service.Add(0));
		Assert.Equal(0, service.Add(-5));
		Assert.Equal(0, service.Value);
	}

	[Fact]
	public void ValueChanged_FiresOnDeath()
	{
		BlackMagicService service = new();
		int lastValue = -1;
		service.ValueChanged += value => lastValue = value;

		service.OnUnitDied(true, "Human");

		Assert.Equal(BlackMagicService.DefaultFriendlyDeathYield, lastValue);
	}

	[Fact]
	public void TrySpend_EnoughValue_SpendsAndFires()
	{
		BlackMagicService service = new();
		int lastValue = -1;
		service.ValueChanged += value => lastValue = value;
		service.Add(30);

		bool spent = service.TrySpend(10);

		Assert.True(spent);
		Assert.Equal(20, service.Value);
		Assert.Equal(20, lastValue);
	}

	[Fact]
	public void TrySpend_InsufficientValue_ReturnsFalse()
	{
		BlackMagicService service = new();
		service.Add(5);

		bool spent = service.TrySpend(10);

		Assert.False(spent);
		Assert.Equal(5, service.Value);
	}

	[Fact]
	public void Reset_ZeroesValue()
	{
		BlackMagicService service = new();
		int lastValue = -1;
		service.ValueChanged += value => lastValue = value;
		service.Add(25);

		service.Reset();

		Assert.Equal(0, service.Value);
		Assert.Equal(0, lastValue);
	}

	[Fact]
	public void OnUnitDied_UsesInjectedConfig_RealBug_HardcodedYields()
	{
		BlackMagicService service = new(new FakeConfig(50, 20));

		service.OnUnitDied(true, "Human");
		service.OnUnitDied(false, "Orc");

		Assert.Equal(70, service.Value);
	}

	private sealed class FakeConfig : INecromancerBalanceConfig
	{
		public FakeConfig(int friendlyDeathYield, int enemyDeathYield)
		{
			FriendlyDeathYield = friendlyDeathYield;
			EnemyDeathYield = enemyDeathYield;
		}

		public int FriendlyDeathYield { get; }
		public int EnemyDeathYield { get; }
	}
}