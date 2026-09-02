using CasualCastle.Application;
using CasualCastle.Domain.Battle;
using CasualCastle.Domain.Building;
using CasualCastle.Domain.History;
using CasualCastle.Domain.Shared;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace CasualCastle.Application.Tests;

public class PhaseTransitionUseCaseTests
{
	private static PhaseTransitionUseCase CreateUseCase(RecordingQuery query, RecordingCommand command, RecordingShop shop, RecordingCapture capture, Mock<IGameState> gameState, Mock<IReplayUseCase> replay, Mock<IGameSessionService> session)
	{
		GameSessionPorts ports = TestPorts.Create(query, command, shop, capture);
		NightCombineUseCase nightCombine = new(replay.Object);
		return new PhaseTransitionUseCase(ports, gameState.Object, nightCombine, session.Object);
	}

	[Fact]
	public void CombatToShop_CapturesDaySnapshotAndRefreshesShop()
	{
		RecordingQuery query = new() { CurrentNightIndex = 1, Gold = 10 };
		RecordingShop shop = new();
		RecordingCapture capture = new();
		Mock<IGameState> gameState = new();
		gameState.SetupGet(s => s.IsPlaying).Returns(true);
		PhaseTransitionUseCase useCase = CreateUseCase(query, new RecordingCommand(), shop, capture, gameState, new Mock<IReplayUseCase>(), new Mock<IGameSessionService>());

		useCase.Handle(MatchPhase.Combat, MatchPhase.Shop, null);

		Assert.Single(capture.Snapshots);
		Assert.Equal(BattlePhase.Day, capture.Snapshots[0].Phase);
		Assert.Equal(1, capture.Snapshots[0].NightIndex);
		Assert.Equal(10, capture.Snapshots[0].Gold);
		Assert.True(shop.Available);
		Assert.Equal(1, shop.RefreshCalls);
		Assert.Equal(1, shop.OpenCalls);
	}

	[Fact]
	public void SamePhaseStart_DoesNotCaptureSnapshotOrSaveGame()
	{
		RecordingQuery query = new() { CurrentNightIndex = 0 };
		RecordingCapture capture = new();
		RecordingShop shop = new();
		Mock<IReplayUseCase> replay = new();
		Mock<IGameState> gameState = new();
		gameState.SetupGet(s => s.IsPlaying).Returns(true);
		PhaseTransitionUseCase useCase = CreateUseCase(query, new RecordingCommand(), shop, capture, gameState, replay, new Mock<IGameSessionService>());
		IReplayTarget target = new Mock<IReplayTarget>().Object;

		useCase.Handle(MatchPhase.Combat, MatchPhase.Combat, target);

		Assert.Empty(capture.Snapshots);
		Assert.True(shop.Available);
		replay.Verify(r => r.ApplyNightSnapshot(target, 1, BattlePhase.Day), Times.Once);
	}

	[Fact]
	public void ShopToCombat_CapturesNightAppliesReplayAndSavesGame()
	{
		RecordingQuery query = new() { CurrentNightIndex = 2 };
		RecordingCapture capture = new();
		Mock<IReplayUseCase> replay = new();
		Mock<IGameSessionService> session = new();
		Mock<IGameState> gameState = new();
		gameState.SetupGet(s => s.IsPlaying).Returns(true);
		PhaseTransitionUseCase useCase = CreateUseCase(query, new RecordingCommand(), new RecordingShop(), capture, gameState, replay, session);
		IReplayTarget target = new Mock<IReplayTarget>().Object;

		useCase.Handle(MatchPhase.Shop, MatchPhase.Combat, target);

		Assert.Single(capture.Snapshots);
		Assert.Equal(BattlePhase.Night, capture.Snapshots[0].Phase);
		replay.Verify(r => r.ApplyNightSnapshot(target, 3, BattlePhase.Day), Times.Once);
		session.Verify(s => s.SaveGame(0), Times.Once);
	}

	[Fact]
	public void ShopToCombat_NightZero_DoesNotSaveGame()
	{
		RecordingQuery query = new() { CurrentNightIndex = 0 };
		RecordingCapture capture = new();
		Mock<IReplayUseCase> replay = new();
		Mock<IGameSessionService> session = new();
		Mock<IGameState> gameState = new();
		gameState.SetupGet(s => s.IsPlaying).Returns(true);
		PhaseTransitionUseCase useCase = CreateUseCase(query, new RecordingCommand(), new RecordingShop(), capture, gameState, replay, session);

		useCase.Handle(MatchPhase.Shop, MatchPhase.Combat, null);

		session.Verify(s => s.SaveGame(It.IsAny<int>()), Times.Never);
	}
}
