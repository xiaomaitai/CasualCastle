using Godot;
using System;

public sealed class GameOverUiController
{
    private readonly ColorRect _overlay;
    private readonly Panel _panel;
    private readonly Label _label;
    private readonly Button _backToTitleButton;
    private readonly Action _goToTitle;
    private readonly ConfirmationDialog _saveReportDialog;

    public GameOverUiController(CanvasLayer uiRoot, Action goToTitle)
    {
        _overlay = uiRoot.GetNode<ColorRect>("GameOverOverlay");
        _panel = uiRoot.GetNode<Panel>("GameOverPanel");
        _label = uiRoot.GetNode<Label>("GameOverPanel/GameOverLabel");
        _backToTitleButton = uiRoot.GetNode<Button>("GameOverPanel/BackToTitleButton");
        _goToTitle = goToTitle;
        _saveReportDialog = new ConfirmationDialog
        {
            Title = "记录战报",
            DialogText = "是否将本局战报保存为永久记录？",
            OkButtonText = "保存并返回标题",
        };
        _saveReportDialog.GetCancelButton().Text = "不保存并返回标题";
        uiRoot.AddChild(_saveReportDialog);

        _backToTitleButton.Pressed += OnBackToTitlePressed;
        _saveReportDialog.Confirmed += OnSaveReportConfirmed;
        _saveReportDialog.Canceled += OnDiscardReportConfirmed;
    }

    public void Dispose()
    {
        _backToTitleButton.Pressed -= OnBackToTitlePressed;
        _saveReportDialog.Confirmed -= OnSaveReportConfirmed;
        _saveReportDialog.Canceled -= OnDiscardReportConfirmed;
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
        if (BattleReportSystem.Instance?.HasCurrentSnapshots == true)
        {
            _saveReportDialog.PopupCentered();
            return;
        }

        _goToTitle();
    }

    private void OnSaveReportConfirmed()
    {
        BattleReportSystem.Instance?.SaveCurrentReport();
        _goToTitle();
    }

    private void OnDiscardReportConfirmed()
    {
        BattleReportSystem.Instance?.DiscardCurrentReport();
        _goToTitle();
    }
}
