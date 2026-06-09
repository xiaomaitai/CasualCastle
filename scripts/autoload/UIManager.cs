using Godot;
using System;

public partial class UIManager : Node2D
{
    public static UIManager Instance { get; private set; }

    private ProgressBar _playerHealthBar;
    private ProgressBar _enemyHealthBar;
    private Panel _gameOverPanel;
    private Label _gameOverLabel;
    private Button _restartButton;

    public override void _Ready()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeUI();
        }
        else
        {
            QueueFree();
        }
    }

    private void InitializeUI()
    {
        var uiRoot = GetNodeOrNull<CanvasLayer>("/root/MainGame/UI");
        if (uiRoot != null)
        {
            _playerHealthBar = uiRoot.GetNode<ProgressBar>("PlayerHealthBar");
            _enemyHealthBar = uiRoot.GetNode<ProgressBar>("EnemyHealthBar");
            _gameOverPanel = uiRoot.GetNode<Panel>("GameOverPanel");
            _gameOverLabel = uiRoot.GetNode<Label>("GameOverPanel/GameOverLabel");
            _restartButton = uiRoot.GetNode<Button>("GameOverPanel/RestartButton");

            _restartButton?.Connect("pressed", Callable.From(RestartGame));
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.Connect(nameof(GameManager.PlayerHealthChangedEventHandler), Callable.From(UpdatePlayerHealth));
            GameManager.Instance.Connect(nameof(GameManager.EnemyHealthChangedEventHandler), Callable.From(UpdateEnemyHealth));
            GameManager.Instance.Connect(nameof(GameManager.GameStateChangedEventHandler), Callable.From(OnGameStateChanged));
        }
    }

    public void UpdatePlayerHealth(int health)
    {
        _playerHealthBar?.SetValue(health);
    }

    public void UpdateEnemyHealth(int health)
    {
        _enemyHealthBar?.SetValue(health);
    }

    public void OnGameStateChanged(GameManager.GameState state)
    {
        if (_gameOverPanel != null)
        {
            _gameOverPanel.Visible = state == GameManager.GameState.GameOver;
            if (_gameOverLabel != null)
            {
                _gameOverLabel.Text = GameManager.Instance.PlayerHealth > 0 ? "Victory!" : "Defeat!";
            }
        }
    }

    private void RestartGame()
    {
        GameManager.Instance?.RestartGame();
    }
}
