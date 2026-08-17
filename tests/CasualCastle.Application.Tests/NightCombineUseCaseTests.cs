using CasualCastle.Application;
using CasualCastle.Domain.Battle;
using CasualCastle.Domain.Building;
using CasualCastle.Domain.History;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace CasualCastle.Application.Tests;

public class NightCombineUseCaseTests
{
	[Fact]
	public void ResolveNightCombines_Necromancer_UsesRitualCoreLevel()
	{
		Mock<ICombineUseCase> combine = new();
		NightCombineUseCase useCase = new(new Mock<IBuildingRepository>().Object, new Mock<IReplayUseCase>().Object, combine.Object);
		List<IBuildingState> buildings = new() { new FakeBuildingState("RitualCircleL2", 0, 0) };

		useCase.ResolveNightCombines(buildings, FactionDef.NecromancerId, true, true, new FakeCombineFactory(), () => { });

		combine.Verify(c => c.ResolveCombines(buildings, true, true, true, It.IsAny<ICombineBuildingFactory>(), It.IsAny<Action<IBuildingState>>(), 2), Times.Once);
	}

	[Fact]
	public void ResolveNightCombines_OtherFaction_UsesMaxCoreLevel()
	{
		Mock<ICombineUseCase> combine = new();
		NightCombineUseCase useCase = new(new Mock<IBuildingRepository>().Object, new Mock<IReplayUseCase>().Object, combine.Object);

		useCase.ResolveNightCombines(new List<IBuildingState>(), FactionDef.HordeId, true, false, new FakeCombineFactory(), () => { });

		combine.Verify(c => c.ResolveCombines(It.IsAny<List<IBuildingState>>(), true, true, false, It.IsAny<ICombineBuildingFactory>(), It.IsAny<Action<IBuildingState>>(), int.MaxValue), Times.Once);
	}

	[Fact]
	public void ApplyReplaySnapshot_NullTarget_Skips()
	{
		Mock<IReplayUseCase> replay = new();
		NightCombineUseCase useCase = new(new Mock<IBuildingRepository>().Object, replay.Object, new Mock<ICombineUseCase>().Object);
		int applied = 0;

		useCase.ApplyReplaySnapshot(null, 1, BattlePhase.Day, () => applied++);

		replay.Verify(r => r.ApplyNightSnapshot(It.IsAny<IReplayTarget>(), It.IsAny<int>(), It.IsAny<BattlePhase>()), Times.Never);
		Assert.Equal(0, applied);
	}

	[Fact]
	public void ApplyReplaySnapshot_AppliesThenInvokesCallback()
	{
		Mock<IReplayUseCase> replay = new();
		NightCombineUseCase useCase = new(new Mock<IBuildingRepository>().Object, replay.Object, new Mock<ICombineUseCase>().Object);
		IReplayTarget target = new Mock<IReplayTarget>().Object;
		int applied = 0;

		useCase.ApplyReplaySnapshot(target, 4, BattlePhase.Night, () => applied++);

		replay.Verify(r => r.ApplyNightSnapshot(target, 4, BattlePhase.Night), Times.Once);
		Assert.Equal(1, applied);
	}
}
