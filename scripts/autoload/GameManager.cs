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

    public enum GameState
    {
        Playing,
        GameOver
    }

    public GameState CurrentState { get; private set; } = GameState.Playing;

    [Export]
    public int PlayerMaxHealth = 100;

    [Export]
    public int EnemyMaxHealth = 100;

    public int PlayerHealth { get; private set; }
    public int EnemyHealth { get; private set; }

    private Barracks _playerBarracks;

    public override void _Ready()
    {
        Instance = this;
        PlayerHealth = PlayerMaxHealth;
        EnemyHealth = EnemyMaxHealth;
        CallDeferred(MethodName.BindPlayerBarracks);
    }

    private void BindPlayerBarracks()
    {
        _playerBarracks = GetNodeOrNull<Barracks>("Battlefield/PlayerSide/PlayerCastle/Barracks");
    }

    public override void _Input(InputEvent @event)
    {
        if (CurrentState != GameState.Playing) return;
        if (@event is not InputEventKey keyEvent || !keyEvent.Pressed || keyEvent.Echo) return;
        if (keyEvent.Keycode != Key.P) return;

        _playerBarracks ??= GetNodeOrNull<Barracks>("Battlefield/PlayerSide/PlayerCastle/Barracks");
        _playerBarracks?.SpawnUnits(10);
        GetViewport().SetInputAsHandled();
    }

    public override void _ExitTree()
    {
        if (Instance == this)
            Instance = null;
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
        EmitSignal(SignalName.GameStateChanged, (int)CurrentState);
        UIManager.Instance?.OnGameStateChanged(GameState.GameOver);
        GD.Print(playerWon ? "Player Wins!" : "Enemy Wins!");
    }

    public void RestartGame()
    {
        CurrentState = GameState.Playing;
    }
}
