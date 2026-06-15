using Godot;
using System;

public partial class UIManager : Node2D
{
    public static UIManager Instance { get; private set; }

    private ProgressBar _playerHealthBar;
    private ProgressBar _enemyHealthBar;
    private Panel _gameOverPanel;
    private Label _gameOverLabel;
    private Button _backToTitleButton;

    private const string TitleScene = "res://scenes/ui/title_screen.tscn";

    public override void _Ready()
    {
        Instance = this;
        InitializeUI();
    }

    public override void _ExitTree()
    {
        if (Instance == this)
            Instance = null;
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
            _backToTitleButton = uiRoot.GetNode<Button>("GameOverPanel/BackToTitleButton");

            _backToTitleButton?.Connect("pressed", Callable.From(GoToTitle));
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.Connect(nameof(GameManager.PlayerHealthChangedEventHandler), Callable.From((int health) => UpdatePlayerHealth(health)));
            GameManager.Instance.Connect(nameof(GameManager.EnemyHealthChangedEventHandler), Callable.From((int health) => UpdateEnemyHealth(health)));
            GameManager.Instance.Connect(nameof(GameManager.GameStateChangedEventHandler), Callable.From((GameManager.GameState state) => OnGameStateChanged(state)));
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
                _gameOverLabel.Text = GameManager.Instance.PlayerHealth > 0 ? "胜利！" : "失败！";
            }
        }
    }

    private void GoToTitle()
    {
        GetTree().ChangeSceneToFile(TitleScene);
    }
}
