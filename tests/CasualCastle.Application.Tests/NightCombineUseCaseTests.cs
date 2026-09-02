using CasualCastle.Application;
using CasualCastle.Domain.Battle;
using CasualCastle.Domain.History;
using Moq;
using System;
using Xunit;

namespace CasualCastle.Application.Tests;

public class NightCombineUseCaseTests
{
	[Fact]
	public void ApplyReplaySnapshot_NullTarget_Skips()
	{
		Mock<IReplayUseCase> replay = new();
		NightCombineUseCase useCase = new(replay.Object);
		int applied = 0;

		useCase.ApplyReplaySnapshot(null, 1, BattlePhase.Day, () => applied++);

		replay.Verify(r => r.ApplyNightSnapshot(It.IsAny<IReplayTarget>(), It.IsAny<int>(), It.IsAny<BattlePhase>()), Times.Never);
		Assert.Equal(0, applied);
	}

	[Fact]
	public void ApplyReplaySnapshot_AppliesThenInvokesCallback()
	{
		Mock<IReplayUseCase> replay = new();
		NightCombineUseCase useCase = new(replay.Object);
		IReplayTarget target = new Mock<IReplayTarget>().Object;
		int applied = 0;

		useCase.ApplyReplaySnapshot(target, 4, BattlePhase.Night, () => applied++);

		replay.Verify(r => r.ApplyNightSnapshot(target, 4, BattlePhase.Night), Times.Once);
		Assert.Equal(1, applied);
	}
}
