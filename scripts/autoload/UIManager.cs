using Godot;
using System;

public partial class UIManager : Node2D
{
    public static UIManager Instance { get; private set; }

    private ProgressBar _playerHealthBar;
    private ProgressBar _enemyHealthBar;
    private ColorRect _gameOverOverlay;
    private Panel _gameOverPanel;
    private Label _gameOverLabel;
    private Button _backToTitleButton;
    private Label _phaseLabel;
    private Label _phaseTimerLabel;
    private Button _skipPhaseButton;

    private const string TitleScene = "res://scenes/ui/title_screen.tscn";

    public override void _Ready()
    {
        Instance = this;
        SetProcess(true);
        InitializeUI();
    }

    public override void _ExitTree()
    {
        if (Instance == this)
            Instance = null;
    }

    public override void _Process(double _)
    {
        UpdatePhaseDisplay();
    }

    private void InitializeUI()
    {
        var uiRoot = GetParent()?.GetNodeOrNull<CanvasLayer>("UI");
        if (uiRoot == null) return;

        _playerHealthBar = uiRoot.GetNode<ProgressBar>("PlayerHealthBar");
        _enemyHealthBar = uiRoot.GetNode<ProgressBar>("EnemyHealthBar");
        _gameOverOverlay = uiRoot.GetNode<ColorRect>("GameOverOverlay");
        _gameOverPanel = uiRoot.GetNode<Panel>("GameOverPanel");
        _gameOverLabel = uiRoot.GetNode<Label>("GameOverPanel/GameOverLabel");
        _backToTitleButton = uiRoot.GetNode<Button>("GameOverPanel/BackToTitleButton");
        _phaseLabel = uiRoot.GetNode<Label>("PhasePanel/PhaseLabel");
        _phaseTimerLabel = uiRoot.GetNode<Label>("PhasePanel/PhaseTimerLabel");
        _skipPhaseButton = uiRoot.GetNode<Button>("PhasePanel/SkipPhaseButton");

        _backToTitleButton.Pressed += GoToTitle;
        _skipPhaseButton.Pressed += OnSkipPhasePressed;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.PlayerHealthChanged += UpdatePlayerHealth;
            GameManager.Instance.EnemyHealthChanged += UpdateEnemyHealth;
            GameManager.Instance.GameStateChanged += OnGameStateChanged;
            GameManager.Instance.PhaseChanged += OnPhaseChanged;
            UpdatePhaseDisplay();
        }
    }

    private void OnSkipPhasePressed()
    {
        GameManager.Instance?.AdvancePhase();
    }

    private void OnPhaseChanged(GameManager.GamePhase phase)
    {
        UpdatePhaseDisplay();
    }

    private void UpdatePhaseDisplay()
    {
        var gm = GameManager.Instance;
        if (gm == null) return;

        if (_phaseLabel != null)
            _phaseLabel.Text = gm.IsDay ? "白天" : "夜晚";

        if (_phaseTimerLabel != null)
            _phaseTimerLabel.Text = FormatTime(gm.PhaseTimeRemaining);
    }

    private static string FormatTime(float seconds)
    {
        int total = Math.Max(0, (int)Math.Ceiling(seconds));
        return $"{total / 60}:{total % 60:D2}";
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
        bool show = state == GameManager.GameState.GameOver;

        if (_gameOverOverlay != null)
            _gameOverOverlay.Visible = show;

        if (_gameOverPanel != null)
            _gameOverPanel.Visible = show;

        if (_skipPhaseButton != null)
            _skipPhaseButton.Visible = !show;

        if (show && _gameOverLabel != null && GameManager.Instance != null)
            _gameOverLabel.Text = GameManager.Instance.PlayerHealth > 0 ? "胜利！" : "失败！";
    }

    private void GoToTitle()
    {
        GetTree().ChangeSceneToFile(TitleScene);
    }
}
