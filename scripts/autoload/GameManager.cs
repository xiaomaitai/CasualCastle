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

    public override void _Ready()
    {
        if (Instance == null)
        {
            Instance = this;
            PlayerHealth = PlayerMaxHealth;
            EnemyHealth = EnemyMaxHealth;
        }
        else
        {
            QueueFree();
        }
    }

    public void TakeDamage(bool isPlayer, int damage)
    {
        if (CurrentState != GameState.Playing) return;

        if (isPlayer)
        {
            PlayerHealth = Math.Max(0, PlayerHealth - damage);
            EmitSignal(nameof(PlayerHealthChangedEventHandler), PlayerHealth);
            
            if (PlayerHealth <= 0)
            {
                EndGame(false);
            }
        }
        else
        {
            EnemyHealth = Math.Max(0, EnemyHealth - damage);
            EmitSignal(nameof(EnemyHealthChangedEventHandler), EnemyHealth);
            
            if (EnemyHealth <= 0)
            {
                EndGame(true);
            }
        }
    }

    public void EndGame(bool playerWon)
    {
        CurrentState = GameState.GameOver;
        EmitSignal(nameof(GameStateChangedEventHandler), (int)CurrentState);
        GD.Print(playerWon ? "Player Wins!" : "Enemy Wins!");
    }

    public void RestartGame()
    {
        PlayerHealth = PlayerMaxHealth;
        EnemyHealth = EnemyMaxHealth;
        CurrentState = GameState.Playing;
        EmitSignal(nameof(GameStateChangedEventHandler), (int)CurrentState);
        EmitSignal(nameof(PlayerHealthChangedEventHandler), PlayerHealth);
        EmitSignal(nameof(EnemyHealthChangedEventHandler), EnemyHealth);
    }
}
