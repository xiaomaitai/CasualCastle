// 游戏总控：对局状态、阶段信号、血量与存档入口
using CasualCastle.Adapters.Godot;
using CasualCastle.Application;
using CasualCastle.Domain.Battle;
using CasualCastle.Domain.Building;
using CasualCastle.Domain.History;
using CasualCastle.Domain.Shared;
using Godot;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

public partial class GameManager : Node2D, IGameState
{
	public static GameManager Instance { get; private set; }
	public static ServiceProvider Services { get; private set; }
	public static T Get<T>() where T : class => Services.GetService<T>();

	bool IGameState.IsPlaying => CurrentState == GameState.Playing;
	bool IGameState.IsCombat => IsCombat;
	bool IGameState.IsPaused => IsPaused;
	int IGameState.CurrentNightIndex => CurrentNightIndex;

	[Signal]
	public delegate void GameStateChangedEventHandler(GameState newState);

	[Signal]
	public delegate void PlayerHealthChangedEventHandler(int health);

	[Signal]
	public delegate void EnemyHealthChangedEventHandler(int health);

	[Signal]
	public delegate void PhaseChangedEventHandler(MatchPhase newPhase);

	public enum GameState
	{
		Playing,
		GameOver
	}


	public GameState CurrentState { get; private set; } = GameState.Playing;
	public MatchPhase CurrentPhase => _phaseMachine.CurrentPhase;
	public bool IsCombat => CurrentPhase == MatchPhase.Combat;
	public bool IsShop => CurrentPhase == MatchPhase.Shop;
	public bool IsCombine => CurrentPhase == MatchPhase.Combine;
	public bool IsUndeadBattle => SelectedFaction == FactionDef.NecromancerId || _pendingEnemyIsNecromancer;
	public bool IsNightDisplayed => CurrentPhase switch
	{
		MatchPhase.Combat => IsUndeadBattle,
		MatchPhase.Shop => !IsUndeadBattle,
		_ => IsUndeadBattle,
	};
	public bool IsDayDisplayed => !IsNightDisplayed;
	public bool IsPaused { get; private set; }
	public float PhaseTimeRemaining => _phaseMachine.PhaseTimeRemaining;
	public int CurrentNightIndex => _phaseMachine.NightIndex;
	public string PendingReplayReportId { get; private set; } = "";
	public int PendingLoadSlot { get; set; } = -1;
	public string SelectedFaction { get; private set; } = FactionIds.Alliance;

	[Export]
	public int PlayerMaxHealth = 100;

	[Export]
	public int EnemyMaxHealth = 100;

	public int PlayerHealth { get; private set; }
	public int EnemyHealth { get; private set; }
	public Castle PlayerCastle => _playerCastle;
	public Castle EnemyCastle => _enemyCastle;
	public Node2D Battlefield => _battlefield;

	private Node2D _battlefield;
	private Castle _playerCastle;
	private Castle _enemyCastle;

	private readonly MatchPhaseMachine _phaseMachine = new();
	private bool _pendingEnemyIsNecromancer;

	public override void _Ready()
	{
		Instance = this;
		Services = CasualCastle.CompositionRoot.Build();
		Services.GetRequiredService<GameStateProvider>().Current = this;
		AdapterRegistry.Register<GameManager>(this);
		AdapterRegistry.Register<IGameState>(this);
		_phaseMachine.PhaseChanged += OnPhaseChanged;
		SetProcess(false);
		SetProcessInput(true);
	}

	public override void _Process(double delta)
	{
		if (CurrentState != GameState.Playing || IsPaused) return;

		_phaseMachine.Tick((float)delta);
	}

	public override void _ExitTree()
	{
		if (Instance == this)
		{
			AdapterRegistry.Unregister<IGameState>(this);
			AdapterRegistry.Unregister<GameManager>(this);
			Services?.Dispose();
			Instance = null;
		}
	}

	public void StartGameSession(Node2D battlefield, Castle playerCastle, Castle enemyCastle)
	{
		_battlefield = battlefield;
		_playerCastle = playerCastle;
		_enemyCastle = enemyCastle;
		_phaseMachine.SetNightIndex(0);

		CurrentState = GameState.Playing;
		EmitSignal(SignalName.GameStateChanged, (int)CurrentState);
		IsPaused = false;
		SyncHeartHealthFromCastles();
		Get<MatchSessionUseCase>().StartNewMatch(SelectedFaction, PendingReplayReportId);

		_phaseMachine.StartMatch();
		SetProcess(true);
	}

	private void SyncHeartHealthFromCastles()
	{
		if (_playerCastle?.Heart != null)
		{
			PlayerMaxHealth = _playerCastle.Heart.MaxHealth;
			PlayerHealth = _playerCastle.Heart.Health;
		}
		else
		{
			PlayerHealth = PlayerMaxHealth;
		}

		if (_enemyCastle?.Heart != null)
		{
			EnemyMaxHealth = _enemyCastle.Heart.MaxHealth;
			EnemyHealth = _enemyCastle.Heart.Health;
		}
		else
		{
			EnemyHealth = EnemyMaxHealth;
		}

		EmitSignal(SignalName.PlayerHealthChanged, PlayerHealth);
		EmitSignal(SignalName.EnemyHealthChanged, EnemyHealth);
	}

	public void ClearGameSession()
	{
		SetProcess(false);
		_battlefield = null;
		_playerCastle = null;
		_enemyCastle = null;
		_phaseMachine.SetNightIndex(0);
	}

	public void SetNightIndex(int index)
	{
		_phaseMachine.SetNightIndex(index);
	}

	public void SetPendingReplayReportId(string reportId)
	{
		PendingReplayReportId = reportId ?? "";
		_pendingEnemyIsNecromancer = Services.GetRequiredService<BattleReportService>().IsNecromancerReport(PendingReplayReportId);
	}

	public void SetSelectedFaction(string factionId)
	{
		SelectedFaction = string.IsNullOrEmpty(factionId) ? FactionIds.Alliance : factionId;
	}

	public void SetPaused(bool paused)
	{
		if (CurrentState != GameState.Playing)
			return;

		IsPaused = paused;
		SetProcess(!paused);
	}

	public void AdvancePhase()
	{
		if (CurrentState != GameState.Playing) return;
		_phaseMachine.Advance();
	}

	private void OnPhaseChanged(MatchPhase previous, MatchPhase next)
	{
		ReplayTarget replayTarget = _enemyCastle == null ? null : new ReplayTarget(_enemyCastle);
		Get<PhaseTransitionUseCase>().Handle(previous, next, replayTarget);

		EmitSignal(SignalName.PhaseChanged, (int)next);
		Sfx.Play(SfxEvents.PhaseChange);
		GD.Print($"Phase: {next}");
	}

	public void OnCastleHeartHealthChanged(bool isPlayer, int health, int maxHealth)
	{
		if (CurrentState != GameState.Playing)
			return;

		if (isPlayer)
		{
			PlayerMaxHealth = maxHealth;
			PlayerHealth = health;
			EmitSignal(SignalName.PlayerHealthChanged, PlayerHealth);

			if (PlayerHealth <= 0)
				EndGame(false);
		}
		else
		{
			EnemyMaxHealth = maxHealth;
			EnemyHealth = health;
			EmitSignal(SignalName.EnemyHealthChanged, EnemyHealth);

			if (EnemyHealth <= 0)
				EndGame(true);
		}
	}

	public void EndGame(bool playerWon)
	{
		CurrentState = GameState.GameOver;
		IsPaused = false;
		SetProcess(false);
		EmitSignal(SignalName.GameStateChanged, (int)CurrentState);
		GD.Print(playerWon ? "Player Wins!" : "Enemy Wins!");
		Sfx.Play(playerWon ? SfxEvents.Victory : SfxEvents.Defeat);

		int slot = 0;
		Get<IGameSessionService>().DeleteSave(slot);
	}

	public void RestartGame()
	{
		CurrentState = GameState.Playing;
		EmitSignal(SignalName.GameStateChanged, (int)CurrentState);
		SetProcess(true);
	}

	public void ResetToTitleState()
	{
		CurrentState = GameState.Playing;
		IsPaused = false;
		SetProcess(false);
		EmitSignal(SignalName.GameStateChanged, (int)CurrentState);
	}
}
