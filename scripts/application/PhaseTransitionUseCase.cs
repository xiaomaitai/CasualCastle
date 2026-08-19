// 阶段流转用例：快照、商店、夜间结算、复刻与自动存档的单一触发入口
using CasualCastle.Domain.Battle;
using CasualCastle.Domain.Building;
using CasualCastle.Domain.History;
using CasualCastle.Domain.Shared;
using System.Collections.Generic;
using System.Linq;

namespace CasualCastle.Application;

public class PhaseTransitionUseCase
{
	private readonly GameSessionPorts _ports;
	private readonly IGameState _gameState;
	private readonly NightCombineUseCase _nightCombine;
	private readonly IGameSessionService _session;

	public PhaseTransitionUseCase(GameSessionPorts ports, IGameState gameState, NightCombineUseCase nightCombine, IGameSessionService session)
	{
		_ports = ports;
		_gameState = gameState;
		_nightCombine = nightCombine;
		_session = session;
	}

	private static List<BuildingSnapshot> MapSnapshots(IReadOnlyList<IBuildingState> buildings)
	{
		List<BuildingSnapshot> snapshots = new();
		foreach (IBuildingState b in buildings)
		{
			if (b.IsDestroyed)
				continue;
			snapshots.Add(new BuildingSnapshot
			{
				TypeId = b.TypeId,
				AnchorGridX = b.AnchorGridX,
				AnchorGridY = b.AnchorGridY,
				Health = b.Health,
				IsManuallyPaused = b.IsManuallyPaused,
				IsCombineProhibited = b.IsCombineProhibited,
			});
		}
		return snapshots;
	}

	public void Handle(MatchPhase previous, MatchPhase next, IReplayTarget enemyTarget)
	{
		IGameSessionQuery query = _ports.Query;
		IShopAccess shop = _ports.Shop;

		if (previous != next)
		{
			List<BuildingSnapshot> buildingSnapshots = MapSnapshots(query.PlayerBuildingStates);
			List<CardSnapshot> cardSnapshots = query.HandCards
				.Select(c => new CardSnapshot { BuildingType = c.BuildingType, Cost = c.Cost })
				.ToList();
			_ports.BattleReportCapture.CapturePhaseSnapshot(buildingSnapshots, query.Gold, cardSnapshots, query.CurrentNightIndex, PhaseMapping.ToBattlePhase(previous));
		}

		shop.SetShopAvailable(next == MatchPhase.Combat || next == MatchPhase.Shop);
		if (next == MatchPhase.Shop)
		{
			shop.RefreshOffers();
			shop.RequestOpenShop();
		}
		if (next == MatchPhase.Combine)
			_nightCombine.ResolveNightCombines(query.PlayerBuildingStates, query.FactionId, true, _gameState.IsPlaying, query.CombineFactory, _ports.Command.RefreshAfterCombine);
		if (next == MatchPhase.Combat)
		{
			if (enemyTarget != null)
				_nightCombine.ApplyReplaySnapshot(enemyTarget, query.CurrentNightIndex + 1, BattlePhase.Day, _ports.Command.RefreshEnemyAdjacency);
			if (query.CurrentNightIndex >= 1)
			{
				_session.SaveGame(0);
				_ports.BattleReportCapture.SaveCurrentReportDaily();
			}
		}
	}
}
