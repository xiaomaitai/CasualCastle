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
	private static PhaseTransitionUseCase CreateUseCase(RecordingQuery query, RecordingCommand command, RecordingShop shop, RecordingCapture capture, Mock<IGameState> gameState, Mock<ICombineUseCase> combine, Mock<IReplayUseCase> replay, Mock<IGameSessionService> session)
	{
		GameSessionPorts ports = TestPorts.Create(query, command, shop, capture);
		NightCombineUseCase nightCombine = new(new Mock<IBuildingRepository>().Object, replay.Object, combine.Object);
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
		PhaseTransitionUseCase useCase = CreateUseCase(query, new RecordingCommand(), shop, capture, gameState, new Mock<ICombineUseCase>(), new Mock<IReplayUseCase>(), new Mock<IGameSessionService>());

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
	public void SamePhaseStart_DoesNotCaptureSnapshotOrAutosave()
	{
		RecordingQuery query = new() { CurrentNightIndex = 0 };
		RecordingCapture capture = new();
		RecordingShop shop = new();
		Mock<IReplayUseCase> replay = new();
		Mock<IGameState> gameState = new();
		gameState.SetupGet(s => s.IsPlaying).Returns(true);
		PhaseTransitionUseCase useCase = CreateUseCase(query, new RecordingCommand(), shop, capture, gameState, new Mock<ICombineUseCase>(), replay, new Mock<IGameSessionService>());
		IReplayTarget target = new Mock<IReplayTarget>().Object;

		useCase.Handle(MatchPhase.Combat, MatchPhase.Combat, target);

		Assert.Empty(capture.Snapshots);
		Assert.Equal(0, capture.DailyCalls);
		Assert.True(shop.Available);
		replay.Verify(r => r.ApplyNightSnapshot(target, 1, BattlePhase.Day), Times.Once);
	}

	[Fact]
	public void ShopToCombine_CapturesNightAndResolvesCombines()
	{
		RecordingQuery query = new() { CurrentNightIndex = 1, FactionId = FactionIds.Alliance };
		query.PlayerBuildingStates.Add(new FakeBuildingState("barracks", 2, 2));
		query.CombineFactory = new FakeCombineFactory();
		RecordingCapture capture = new();
		Mock<ICombineUseCase> combine = new();
		Mock<IGameState> gameState = new();
		gameState.SetupGet(s => s.IsPlaying).Returns(true);
		PhaseTransitionUseCase useCase = CreateUseCase(query, new RecordingCommand(), new RecordingShop(), capture, gameState, combine, new Mock<IReplayUseCase>(), new Mock<IGameSessionService>());

		useCase.Handle(MatchPhase.Shop, MatchPhase.Combine, null);

		Assert.Single(capture.Snapshots);
		Assert.Equal(BattlePhase.Night, capture.Snapshots[0].Phase);
		combine.Verify(c => c.ResolveCombines(It.IsAny<List<IBuildingState>>(), true, true, true, It.IsAny<ICombineBuildingFactory>(), It.IsAny<Action<IBuildingState>>(), int.MaxValue), Times.Once);
	}

	[Fact]
	public void CombineToCombat_CapturesCombineAppliesReplayAndAutosaves()
	{
		RecordingQuery query = new() { CurrentNightIndex = 2 };
		RecordingCapture capture = new();
		Mock<IReplayUseCase> replay = new();
		Mock<IGameSessionService> session = new();
		Mock<IGameState> gameState = new();
		gameState.SetupGet(s => s.IsPlaying).Returns(true);
		PhaseTransitionUseCase useCase = CreateUseCase(query, new RecordingCommand(), new RecordingShop(), capture, gameState, new Mock<ICombineUseCase>(), replay, session);
		IReplayTarget target = new Mock<IReplayTarget>().Object;

		useCase.Handle(MatchPhase.Combine, MatchPhase.Combat, target);

		Assert.Single(capture.Snapshots);
		Assert.Equal(BattlePhase.Combine, capture.Snapshots[0].Phase);
		replay.Verify(r => r.ApplyNightSnapshot(target, 3, BattlePhase.Day), Times.Once);
		session.Verify(s => s.SaveGame(0), Times.Once);
		Assert.Equal(1, capture.DailyCalls);
	}

	[Fact]
	public void CombineToCombat_NightZero_DoesNotAutosave()
	{
		RecordingQuery query = new() { CurrentNightIndex = 0 };
		RecordingCapture capture = new();
		Mock<IReplayUseCase> replay = new();
		Mock<IGameSessionService> session = new();
		Mock<IGameState> gameState = new();
		gameState.SetupGet(s => s.IsPlaying).Returns(true);
		PhaseTransitionUseCase useCase = CreateUseCase(query, new RecordingCommand(), new RecordingShop(), capture, gameState, new Mock<ICombineUseCase>(), replay, session);

		useCase.Handle(MatchPhase.Combine, MatchPhase.Combat, null);

		session.Verify(s => s.SaveGame(It.IsAny<int>()), Times.Never);
		Assert.Equal(0, capture.DailyCalls);
	}
}
