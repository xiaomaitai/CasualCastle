using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Battle;
using CasualCastle.Domain.Building;
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
    bool IGameState.IsDay => IsDay;
    bool IGameState.IsNight => IsNight;
    bool IGameState.IsPaused => IsPaused;
    int IGameState.CurrentNightIndex => CurrentNightIndex;

    [Signal]
    public delegate void GameStateChangedEventHandler(GameState newState);

    [Signal]
    public delegate void PlayerHealthChangedEventHandler(int health);

    [Signal]
    public delegate void EnemyHealthChangedEventHandler(int health);

    [Signal]
    public delegate void PhaseChangedEventHandler(GamePhase newPhase);

    public enum GameState
    {
        Playing,
        GameOver
    }

    public enum GamePhase
    {
        Day,
        Night
    }

    public GameState CurrentState { get; private set; } = GameState.Playing;
    public GamePhase CurrentPhase { get; private set; } = GamePhase.Day;
    public bool IsDay => CurrentPhase == GamePhase.Day;
    public bool IsNight => CurrentPhase == GamePhase.Night;
    public bool IsPaused { get; private set; }
    public float PhaseTimeRemaining { get; private set; }
    public int CurrentNightIndex { get; set; }
    public string PendingReplayReportId { get; private set; } = "";
    public int PendingLoadSlot { get; set; } = -1;

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

    private BattleReportSystem BattleReportSystem => _battleReportSystem ??= AdapterRegistry.Resolve<BattleReportSystem>();
    private BattleReportSystem _battleReportSystem;

    public override void _Ready()
    {
        Instance = this;
        Services = CasualCastle.CompositionRoot.Build();
        Services.GetRequiredService<GameStateProvider>().Current = this;
        AdapterRegistry.Register<GameManager>(this);
        AdapterRegistry.Register<IGameState>(this);
        SetProcess(false);
        SetProcessInput(true);
    }

    public override void _Process(double delta)
    {
        if (CurrentState != GameState.Playing || IsPaused) return;

        PhaseTimeRemaining -= (float)delta;
        if (PhaseTimeRemaining <= 0f)
            AdvancePhase();
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
        CurrentNightIndex = 0;

        CurrentState = GameState.Playing;
        IsPaused = false;
        SyncHeartHealthFromCastles();
        BattleReportSystem?.StartMatch(PendingReplayReportId);

        BeginPhase(GamePhase.Day);
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
        CurrentNightIndex = 0;
    }

    public void SetPendingReplayReportId(string reportId)
    {
        PendingReplayReportId = reportId ?? "";
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
        BeginPhase(CurrentPhase == GamePhase.Day ? GamePhase.Night : GamePhase.Day);
    }

    private void BeginPhase(GamePhase phase)
    {
        if (phase == GamePhase.Day && CurrentPhase == GamePhase.Night)
            BattleReportSystem?.CaptureNightSnapshot(_playerCastle, CurrentNightIndex);

        if (phase == GamePhase.Night)
            CurrentNightIndex++;

        CurrentPhase = phase;
        PhaseTimeRemaining = phase == GamePhase.Day
            ? GameRules.DayDurationSeconds
            : GameRules.NightDurationSeconds;

        EmitSignal(SignalName.PhaseChanged, (int)phase);
        GD.Print(phase == GamePhase.Day ? "Phase: Day" : "Phase: Night");
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

        int slot = 0;
        Get<IGameSessionService>().DeleteSave(slot);
    }

    public void SaveGame(int slot)
    {
        SaveData data = new SaveData
        {
            SlotIndex = slot,
            Gold = AdapterRegistry.Resolve<Shop>().Gold,
            CurrentNightIndex = CurrentNightIndex,
            PendingReplayReportId = PendingReplayReportId,
        };

        List<Building> buildings = _playerCastle.GetBuildings();
        foreach (Building b in buildings)
        {
            data.Buildings.Add(new BuildingSaveEntry
            {
                TypeId = b.TypeId,
                AnchorGridX = b.AnchorGridX,
                AnchorGridY = b.AnchorGridY,
                Health = b.Health,
            });
        }

        IReadOnlyList<CardData> handCards = AdapterRegistry.Resolve<Hand>().Cards;
        foreach (CardData c in handCards)
        {
            data.HandCards.Add(new CardSaveEntry
            {
                Id = c.Id,
                Name = c.Name,
                Cost = c.Cost,
                BuildingType = c.BuildingType,
                Weight = c.Weight,
            });
        }

        Get<IGameSessionService>().SaveGame(data);
        GD.Print($"Game saved to slot {slot}");
    }

    public SaveData LoadSaveData(int slot)
    {
        return Get<IGameSessionService>().LoadSaveData(slot);
    }

    public bool HasSave(int slot)
    {
        return Get<IGameSessionService>().HasSave(slot);
    }

    public void RestartGame()
    {
        CurrentState = GameState.Playing;
        SetProcess(true);
    }
}
