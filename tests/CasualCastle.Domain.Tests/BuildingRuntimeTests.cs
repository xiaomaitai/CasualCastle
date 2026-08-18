using CasualCastle.Domain.Building;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class BuildingRuntimeTests
{
	[Fact]
	public void Restore_SetsHealthAndStateAndRaisesEvent()
	{
		BuildingRuntime runtime = new();
		int events = 0;
		runtime.HealthChanged += (h, m) => events++;

		runtime.Restore(80, 100, true, true);

		Assert.Equal(80, runtime.Health);
		Assert.Equal(100, runtime.MaxHealth);
		Assert.True(runtime.IsManuallyPaused);
		Assert.True(runtime.IsCombineProhibited);
		Assert.False(runtime.IsDestroyed);
		Assert.Equal(1, events);
	}

	[Fact]
	public void TakeDamage_FloorsAtZero_AndMarksDestroyed()
	{
		BuildingRuntime runtime = new();
		runtime.Restore(30, 100, false, false);

		runtime.TakeDamage(50);

		Assert.Equal(0, runtime.Health);
		Assert.True(runtime.IsDestroyed);
		Assert.False(runtime.IsOperational);
	}

	[Fact]
	public void TakeDamage_IgnoresDamageAfterDestroyed()
	{
		BuildingRuntime runtime = new();
		runtime.Restore(10, 100, false, false);
		runtime.TakeDamage(10);
		runtime.TakeDamage(10);

		Assert.Equal(0, runtime.Health);
	}

	[Fact]
	public void Heal_ClampsAtMaxHealth()
	{
		BuildingRuntime runtime = new();
		runtime.Restore(10, 100, false, false);

		runtime.Heal(500);

		Assert.Equal(100, runtime.Health);
		Assert.False(runtime.IsDamaged);
	}

	[Fact]
	public void Heal_DoesNotReviveDestroyed()
	{
		BuildingRuntime runtime = new();
		runtime.Restore(10, 100, false, false);
		runtime.TakeDamage(10);
		runtime.Heal(50);

		Assert.Equal(0, runtime.Health);
		Assert.True(runtime.IsDestroyed);
	}

	[Fact]
	public void PausedBuilding_StopsWorkAndIsNotOperational()
	{
		BuildingRuntime runtime = new();
		runtime.Restore(100, 100, false, false);
		runtime.BeginWork(10f, 50f);
		runtime.SetPaused(true);

		Assert.False(runtime.IsOperational);
		Assert.Equal(0, runtime.TickWork(1f, 1f));
		Assert.Equal(0f, runtime.WorkDone);
	}

	[Fact]
	public void TickWork_AccumulatesAndReturnsCompletedCycles()
	{
		BuildingRuntime runtime = new();
		runtime.Restore(100, 100, false, false);
		runtime.BeginWork(10f, 30f);

		int cycles = runtime.TickWork(1f, 2f);

		Assert.Equal(0, cycles);
		Assert.Equal(20f, runtime.WorkDone);
		Assert.Equal(1, runtime.TickWork(0.5f, 2f));
		Assert.Equal(0f, runtime.WorkDone);
		Assert.True(runtime.HasWorkCycle);
	}

	[Fact]
	public void PauseAndResumeWork_PreserveProgress()
	{
		BuildingRuntime runtime = new();
		runtime.Restore(100, 100, false, false);
		runtime.BeginWork(10f, 50f);
		runtime.TickWork(1f, 1f);
		runtime.PauseWork();

		Assert.Equal(0, runtime.TickWork(5f, 1f));
		Assert.Equal(10f, runtime.WorkDone);

		runtime.ResumeWork();
		Assert.True(runtime.TickWork(5f, 1f) >= 1);
	}

	[Fact]
	public void StopWork_ClearsProductionState()
	{
		BuildingRuntime runtime = new();
		runtime.Restore(100, 100, false, false);
		runtime.BeginWork(10f, 50f);
		runtime.TickWork(1f, 1f);

		runtime.StopWork();

		Assert.False(runtime.HasWorkCycle);
		Assert.False(runtime.HasProduction);
		Assert.Equal(0f, runtime.WorkDone);
	}

	[Fact]
	public void BeginWork_IgnoresInvalidRateOrRequired()
	{
		BuildingRuntime runtime = new();
		runtime.Restore(100, 100, false, false);

		runtime.BeginWork(0f, 50f);
		runtime.BeginWork(10f, 0f);

		Assert.False(runtime.HasWorkCycle);
	}

	[Fact]
	public void Ritual_StartsTicksAndCompletes()
	{
		BuildingRuntime runtime = new();
		runtime.Restore(100, 100, false, false);

		runtime.StartRitual("ritual_a", 10f, true, "book_1", "ritual_b");

		Assert.True(runtime.IsRitualInProgress);
		Assert.Equal("ritual_a", runtime.ActiveRitual.RitualId);
		Assert.False(runtime.TickRitual(5f));
		Assert.True(runtime.TickRitual(5f));
		Assert.True(runtime.ActiveRitual.IsComplete);

		runtime.CompleteRitual();
		Assert.False(runtime.IsRitualInProgress);
	}

	[Fact]
	public void Ritual_IgnoresInvalidInputs()
	{
		BuildingRuntime runtime = new();
		runtime.Restore(100, 100, false, false);

		runtime.StartRitual("", 10f, false, null, null);
		runtime.StartRitual("ritual_a", 0f, false, null, null);

		Assert.False(runtime.IsRitualInProgress);
	}

	[Fact]
	public void ApplyHealthRatio_ClampsToOneAndMax()
	{
		BuildingRuntime runtime = new();
		runtime.Restore(50, 100, false, false);

		runtime.ApplyHealthRatio(0.25f);
		Assert.Equal(25, runtime.Health);

		runtime.ApplyHealthRatio(0f);
		Assert.Equal(1, runtime.Health);

		runtime.ApplyHealthRatio(2f);
		Assert.Equal(100, runtime.Health);
	}
}
