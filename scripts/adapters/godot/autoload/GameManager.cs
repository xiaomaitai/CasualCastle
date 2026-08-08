using CasualCastle.Adapters.Godot;
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
    public delegate void PhaseChangedEventHandler(GamePhase newPhase);

    public enum GameState
    {
        Playing,
        GameOver
    }

    public enum GamePhase
    {
        Combat,
        Combine,
        Shop
    }

    public GameState CurrentState { get; private set; } = GameState.Playing;
    public GamePhase CurrentPhase { get; private set; } = GamePhase.Combat;
    public bool IsCombat => CurrentPhase == GamePhase.Combat;
    public bool IsShop => CurrentPhase == GamePhase.Shop;
    public bool IsCombine => CurrentPhase == GamePhase.Combine;
    public bool IsUndeadBattle => SelectedFaction == FactionDef.NecromancerId || _pendingEnemyIsNecromancer;
    public bool IsNightDisplayed => CurrentPhase switch
    {
        GamePhase.Combat => IsUndeadBattle,
        GamePhase.Shop => !IsUndeadBattle,
        _ => IsUndeadBattle,
    };
    public bool IsDayDisplayed => !IsNightDisplayed;
    public bool IsPaused { get; private set; }
    public float PhaseTimeRemaining { get; private set; }
    public int CurrentNightIndex { get; set; }
    public string PendingReplayReportId { get; private set; } = "";
    public int PendingLoadSlot { get; set; } = -1;
    public string SelectedFaction { get; private set; } = "alliance";

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
    private float _phaseTimerHoldRemaining;
    private bool _pendingEnemyIsNecromancer;

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

        if (CurrentPhase == GamePhase.Combat && _phaseTimerHoldRemaining > 0f)
        {
            _phaseTimerHoldRemaining -= (float)delta;
            return;
        }

        if (CurrentPhase == GamePhase.Combat || CurrentPhase == GamePhase.Combine)
        {
            PhaseTimeRemaining -= (float)delta;
            if (PhaseTimeRemaining <= 0f)
                AdvancePhase();
        }
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
        EmitSignal(SignalName.GameStateChanged, (int)CurrentState);
        IsPaused = false;
        SyncHeartHealthFromCastles();
        BattleReportSystem?.StartMatch(PendingReplayReportId);

        if (SelectedFaction == FactionDef.NecromancerId)
        {
            Get<BlackMagicService>()?.Reset();
            UndeadFactionBuffs.Reset();
        }

        BeginPhase(GamePhase.Combat);
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
        _pendingEnemyIsNecromancer = Get<BattleReportService>()?.IsNecromancerReport(PendingReplayReportId) ?? false;
    }

    public void SetSelectedFaction(string factionId)
    {
        SelectedFaction = string.IsNullOrEmpty(factionId) ? "alliance" : factionId;
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
        GamePhase nextPhase = CurrentPhase switch
        {
            GamePhase.Combat => GamePhase.Shop,
            GamePhase.Shop => GamePhase.Combine,
            _ => GamePhase.Combat,
        };
        BeginPhase(nextPhase);
    }

    private void BeginPhase(GamePhase phase)
    {
        if (phase == GamePhase.Shop && CurrentPhase == GamePhase.Combat)
        {
            CurrentNightIndex++;
            BattleReportSystem?.CapturePhaseSnapshot(_playerCastle, CurrentNightIndex, BattlePhase.Day);
        }
        else if (phase == GamePhase.Combine && CurrentPhase == GamePhase.Shop)
        {
            BattleReportSystem?.CapturePhaseSnapshot(_playerCastle, CurrentNightIndex, BattlePhase.Night);
        }
        else if (phase == GamePhase.Combat && CurrentPhase == GamePhase.Combine)
        {
            BattleReportSystem?.CapturePhaseSnapshot(_playerCastle, CurrentNightIndex, BattlePhase.Combine);
        }

        CurrentPhase = phase;
        PhaseTimeRemaining = phase == GamePhase.Combine
            ? GameRules.CombineDurationSeconds
            : phase == GamePhase.Combat ? GameRules.CombatDurationSeconds : 0f;
        _phaseTimerHoldRemaining = phase == GamePhase.Combat
            ? GameRules.CombatStartTimerHoldSeconds
            : 0f;

        EmitSignal(SignalName.PhaseChanged, (int)phase);
        Sfx.Play(SfxEvents.PhaseChange);
        GD.Print($"Phase: {phase}");
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

    public void SaveGame(int slot)
    {
        SaveData data = new SaveData
        {
            SlotIndex = slot,
            FactionId = AdapterRegistry.Resolve<Shop>()?.FactionId ?? "alliance",
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
