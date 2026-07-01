using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Building;
using Godot;
using System;

public sealed class HudUiController
{
    private readonly ProgressBar _playerHealthBar;
    private readonly ProgressBar _enemyHealthBar;
    private readonly Label _phaseLabel;
    private readonly Label _phaseTimerLabel;
    private readonly Button _skipPhaseButton;
    private readonly Label _goldLabel;

    public HudUiController(CanvasLayer uiRoot)
    {
        _playerHealthBar = uiRoot.GetNode<ProgressBar>("PlayerHealthBar");
        _enemyHealthBar = uiRoot.GetNode<ProgressBar>("EnemyHealthBar");
        _phaseLabel = uiRoot.GetNode<Label>("PhasePanel/PhaseLabel");
        _phaseTimerLabel = uiRoot.GetNode<Label>("PhasePanel/PhaseTimerLabel");
        _skipPhaseButton = uiRoot.GetNode<Button>("PhasePanel/SkipPhaseButton");
        _goldLabel = uiRoot.GetNode<Label>("GoldLabel");

        _skipPhaseButton.Pressed += OnSkipPhasePressed;

        AdapterRegistry.Resolve<GameManager>().PlayerHealthChanged += UpdatePlayerHealth;
        AdapterRegistry.Resolve<GameManager>().EnemyHealthChanged += UpdateEnemyHealth;
        AdapterRegistry.Resolve<GameManager>().PhaseChanged += OnPhaseChanged;

        UpdatePlayerHealth(AdapterRegistry.Resolve<GameManager>().PlayerHealth);
        UpdateEnemyHealth(AdapterRegistry.Resolve<GameManager>().EnemyHealth);
        UpdatePhaseDisplay();

        if (AdapterRegistry.Resolve<Shop>() != null)
        {
            AdapterRegistry.Resolve<Shop>().GoldChanged += UpdateGoldDisplay;
            UpdateGoldDisplay(AdapterRegistry.Resolve<Shop>().Gold);
        }
    }

    public void Dispose()
    {
        _skipPhaseButton.Pressed -= OnSkipPhasePressed;

        AdapterRegistry.Resolve<GameManager>().PlayerHealthChanged -= UpdatePlayerHealth;
        AdapterRegistry.Resolve<GameManager>().EnemyHealthChanged -= UpdateEnemyHealth;
        AdapterRegistry.Resolve<GameManager>().PhaseChanged -= OnPhaseChanged;

        if (AdapterRegistry.Resolve<Shop>() != null)
            AdapterRegistry.Resolve<Shop>().GoldChanged -= UpdateGoldDisplay;
    }

    public void Process()
    {
        UpdatePhaseDisplay();
    }

    public void SetGameOverVisible(bool visible)
    {
        _skipPhaseButton.Visible = !visible;
    }

    private void OnSkipPhasePressed()
    {
        AdapterRegistry.Resolve<GameManager>().AdvancePhase();
    }

    private void OnPhaseChanged(GameManager.GamePhase phase)
    {
        UpdatePhaseDisplay();
    }

    private void UpdatePlayerHealth(int health)
    {
        _playerHealthBar.MaxValue = AdapterRegistry.Resolve<GameManager>().PlayerMaxHealth;
        _playerHealthBar.SetValue(health);
    }

    private void UpdateEnemyHealth(int health)
    {
        _enemyHealthBar.MaxValue = AdapterRegistry.Resolve<GameManager>().EnemyMaxHealth;
        _enemyHealthBar.SetValue(health);
    }

    private void UpdateGoldDisplay(int gold)
    {
        _goldLabel.Text = $"金币：{gold}";
    }

    private void UpdatePhaseDisplay()
    {
        _phaseLabel.Text = AdapterRegistry.Resolve<GameManager>().IsDay ? "白天" : "夜晚";
        _phaseTimerLabel.Text = FormatTime(AdapterRegistry.Resolve<GameManager>().PhaseTimeRemaining);
    }

    private static string FormatTime(float seconds)
    {
        int total = Math.Max(0, (int)Math.Ceiling(seconds));
        return $"{total / 60}:{total % 60:D2}";
    }
}
