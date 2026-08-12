using CasualCastle.Domain.Battle;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class ManaServiceTests
{
	[Fact]
	public void Add_AccumulatesUpToMax()
	{
		ManaService mana = new();

		mana.Add(40f);
		mana.Add(80f);

		Assert.Equal(ManaService.MaxMana, mana.Value);
	}

	[Fact]
	public void TrySpend_SpendsWhenAvailable()
	{
		ManaService mana = new();
		mana.Add(50f);

		bool spent = mana.TrySpend(30f);

		Assert.True(spent);
		Assert.Equal(20f, mana.Value);
	}

	[Fact]
	public void TrySpend_Insufficient_ReturnsFalse()
	{
		ManaService mana = new();
		mana.Add(10f);

		Assert.False(mana.TrySpend(20f));
		Assert.Equal(10f, mana.Value);
	}

	[Fact]
	public void Reset_ClearsValue()
	{
		ManaService mana = new();
		mana.Add(60f);

		mana.Reset();

		Assert.Equal(0f, mana.Value);
	}
}