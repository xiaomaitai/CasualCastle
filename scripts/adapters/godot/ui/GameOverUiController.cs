using CasualCastle.Adapters.Godot;
using Godot;
using System;

public sealed class GameOverUiController
{
    private readonly ColorRect _overlay;
    private readonly Panel _panel;
    private readonly Label _label;
    private readonly Button _saveReportButton;
    private readonly Button _discardAndBackButton;
    private readonly Action _goToTitle;

    public GameOverUiController(CanvasLayer uiRoot, Action goToTitle)
    {
        _overlay = uiRoot.GetNode<ColorRect>("GameOverOverlay");
        _panel = uiRoot.GetNode<Panel>("GameOverPanel");
        _label = uiRoot.GetNode<Label>("GameOverPanel/GameOverLabel");
        _saveReportButton = uiRoot.GetNode<Button>("GameOverPanel/SaveReportButton");
        _discardAndBackButton = uiRoot.GetNode<Button>("GameOverPanel/DiscardAndBackButton");
        _goToTitle = goToTitle;

        _saveReportButton.Pressed += OnSaveReportConfirmed;
        _discardAndBackButton.Pressed += OnDiscardReportConfirmed;
    }

    public void Dispose()
    {
        _saveReportButton.Pressed -= OnSaveReportConfirmed;
        _discardAndBackButton.Pressed -= OnDiscardReportConfirmed;
    }

    public void SetState(GameManager.GameState state)
    {
        bool show = state == GameManager.GameState.GameOver;
        _overlay.Visible = show;
        _panel.Visible = show;

        if (show)
        {
            _label.Text = AdapterRegistry.Resolve<GameManager>().PlayerHealth > 0 ? "胜利！" : "失败！";
            bool hasSnapshots = AdapterRegistry.Resolve<BattleReportSystem>()?.HasCurrentSnapshots == true;
            _saveReportButton.Visible = hasSnapshots;
            _discardAndBackButton.Text = hasSnapshots ? "不保存并返回标题" : "返回标题";
        }
    }

    private void OnSaveReportConfirmed()
    {
        AdapterRegistry.Resolve<BattleReportSystem>()?.SaveCurrentReport();
        _goToTitle();
    }

    private void OnDiscardReportConfirmed()
    {
        AdapterRegistry.Resolve<BattleReportSystem>()?.DiscardCurrentReport();
        _goToTitle();
    }
}
