using Godot;
using System;

public partial class GameManager : Node2D
{
    public static GameManager Instance { get; private set; }

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
    public float PhaseTimeRemaining { get; private set; }

    [Export]
    public int PlayerMaxHealth = 100;

    [Export]
    public int EnemyMaxHealth = 100;

    public int PlayerHealth { get; private set; }
    public int EnemyHealth { get; private set; }
    public Castle PlayerCastle => _playerCastle;

    private Node2D _battlefield;
    private Castle _playerCastle;
    private int _cheatSpawnCount;

    public override void _Ready()
    {
        Instance = this;
        PlayerHealth = PlayerMaxHealth;
        EnemyHealth = EnemyMaxHealth;
        SetProcess(true);
        SetProcessInput(true);
        var root = GetParent();
        _battlefield = root.GetNode<Node2D>("Battlefield");
        _playerCastle = root.GetNode<Castle>("Battlefield/PlayerSide/PlayerCastle");
        BeginPhase(GamePhase.Day);
    }

    public override void _Process(double delta)
    {
        if (CurrentState != GameState.Playing) return;

        PhaseTimeRemaining -= (float)delta;
        if (PhaseTimeRemaining <= 0f)
            AdvancePhase();
    }

    public override void _Input(InputEvent @event)
    {
        if (CurrentState != GameState.Playing) return;
        if (@event is not InputEventKey keyEvent || !keyEvent.Pressed || keyEvent.Echo) return;
        if (!IsCheatKey(keyEvent)) return;

        SpawnCheatSoldiers(10);
        GetViewport().SetInputAsHandled();
    }

    private static bool IsCheatKey(InputEventKey keyEvent)
    {
        return keyEvent.Keycode == Key.P
            || keyEvent.PhysicalKeycode == Key.P
            || keyEvent.Unicode == 'p'
            || keyEvent.Unicode == 'P';
    }

    private void SpawnCheatSoldiers(int count)
    {
        if (_battlefield == null || _playerCastle == null || count <= 0) return;

        PackedScene soldierScene = GD.Load<PackedScene>("res://prefabs/Soldier.tscn");
        if (soldierScene == null) return;

        const int spawnGridX = 7;
        const int spawnGridY = 4;

        for (int i = 0; i < count; i++)
        {
            Vector2 spawnLocal = _playerCastle.GetBuildingSpawnPosition(
                spawnGridX, spawnGridY, Vector2I.Right, _cheatSpawnCount);
            _cheatSpawnCount++;

            Soldier soldier = soldierScene.Instantiate<Soldier>();
            soldier.GlobalPosition = _playerCastle.ToGlobal(spawnLocal);
            soldier.IsPlayerUnit = true;
            _battlefield.AddChild(soldier);
        }

        GD.Print($"[Cheat] Spawned {count} soldiers");
    }

    public override void _ExitTree()
    {
        if (Instance == this)
            Instance = null;
    }

    public bool CanUnitWork(bool hasNightCombat)
    {
        if (CurrentState != GameState.Playing) return false;
        if (CurrentPhase == GamePhase.Day) return true;
        return hasNightCombat;
    }

    public void AdvancePhase()
    {
        if (CurrentState != GameState.Playing) return;
        BeginPhase(CurrentPhase == GamePhase.Day ? GamePhase.Night : GamePhase.Day);
    }

    private void BeginPhase(GamePhase phase)
    {
        CurrentPhase = phase;
        PhaseTimeRemaining = phase == GamePhase.Day
            ? GameConfig.DayDurationSeconds
            : GameConfig.NightDurationSeconds;
        EmitSignal(SignalName.PhaseChanged, (int)phase);
        GD.Print(phase == GamePhase.Day ? "Phase: Day" : "Phase: Night");
    }

    public void TakeDamage(bool isPlayer, int damage)
    {
        if (CurrentState != GameState.Playing) return;

        if (isPlayer)
        {
            PlayerHealth = Math.Max(0, PlayerHealth - damage);
            EmitSignal(SignalName.PlayerHealthChanged, PlayerHealth);

            if (PlayerHealth <= 0)
                EndGame(false);
        }
        else
        {
            EnemyHealth = Math.Max(0, EnemyHealth - damage);
            EmitSignal(SignalName.EnemyHealthChanged, EnemyHealth);

            if (EnemyHealth <= 0)
                EndGame(true);
        }
    }

    public void EndGame(bool playerWon)
    {
        CurrentState = GameState.GameOver;
        SetProcess(false);
        EmitSignal(SignalName.GameStateChanged, (int)CurrentState);
        UIManager.Instance?.OnGameStateChanged(GameState.GameOver);
        GD.Print(playerWon ? "Player Wins!" : "Enemy Wins!");
    }

    public void RestartGame()
    {
        CurrentState = GameState.Playing;
        SetProcess(true);
    }
}
