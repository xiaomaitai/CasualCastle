using Godot;
using System;

public sealed class GameOverUiController
{
    private readonly ColorRect _overlay;
    private readonly Panel _panel;
    private readonly Label _label;
    private readonly Button _backToTitleButton;
    private readonly Action _goToTitle;

    public GameOverUiController(CanvasLayer uiRoot, Action goToTitle)
    {
        _overlay = uiRoot.GetNode<ColorRect>("GameOverOverlay");
        _panel = uiRoot.GetNode<Panel>("GameOverPanel");
        _label = uiRoot.GetNode<Label>("GameOverPanel/GameOverLabel");
        _backToTitleButton = uiRoot.GetNode<Button>("GameOverPanel/BackToTitleButton");
        _goToTitle = goToTitle;

        _backToTitleButton.Pressed += OnBackToTitlePressed;
    }

    public void Dispose()
    {
        _backToTitleButton.Pressed -= OnBackToTitlePressed;
    }

    public void SetState(GameManager.GameState state)
    {
        bool show = state == GameManager.GameState.GameOver;
        _overlay.Visible = show;
        _panel.Visible = show;

        if (show)
            _label.Text = GameManager.Instance.PlayerHealth > 0 ? "胜利！" : "失败！";
    }

    private void OnBackToTitlePressed()
    {
        _goToTitle();
    }
}
