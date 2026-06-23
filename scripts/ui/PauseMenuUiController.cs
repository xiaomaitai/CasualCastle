using Godot;
using System;

public sealed class PauseMenuUiController
{
    private readonly ColorRect _overlay;
    private readonly Panel _panel;
    private readonly Button _menuButton;
    private readonly Button _continueButton;
    private readonly Button _backToTitleButton;
    private readonly Button _settingsButton;
    private readonly Action _goToTitle;

    private bool _gameOver;

    public bool IsOpen { get; private set; }
    public event Action<bool> OpenChanged;

    public PauseMenuUiController(CanvasLayer uiRoot, Action goToTitle)
    {
        _overlay = uiRoot.GetNode<ColorRect>("PauseOverlay");
        _panel = uiRoot.GetNode<Panel>("PausePanel");
        _menuButton = uiRoot.GetNode<Button>("MenuButton");
        _continueButton = uiRoot.GetNode<Button>("PausePanel/ContinueButton");
        _backToTitleButton = uiRoot.GetNode<Button>("PausePanel/BackToTitleButton");
        _settingsButton = uiRoot.GetNode<Button>("PausePanel/SettingsButton");
        _goToTitle = goToTitle;

        _menuButton.Pressed += OnMenuButtonPressed;
        _continueButton.Pressed += OnContinuePressed;
        _backToTitleButton.Pressed += OnBackToTitlePressed;

        Hide();
    }

    public void Dispose()
    {
        _menuButton.Pressed -= OnMenuButtonPressed;
        _continueButton.Pressed -= OnContinuePressed;
        _backToTitleButton.Pressed -= OnBackToTitlePressed;
    }

    public void SetGameOver(bool gameOver)
    {
        _gameOver = gameOver;
        _menuButton.Disabled = gameOver;

        if (gameOver)
            Close();
    }

    public bool Open()
    {
        if (IsOpen || _gameOver || GameManager.Instance?.CurrentState != GameManager.GameState.Playing)
            return false;

        IsOpen = true;
        _overlay.Visible = true;
        _panel.Visible = true;
        GameManager.Instance.SetPaused(true);
        OpenChanged?.Invoke(true);
        return true;
    }

    public bool Close()
    {
        if (!IsOpen)
            return false;

        IsOpen = false;
        Hide();
        GameManager.Instance?.SetPaused(false);
        OpenChanged?.Invoke(false);
        return true;
    }

    public bool Toggle()
    {
        return IsOpen ? Close() : Open();
    }

    private void OnMenuButtonPressed() => Open();

    private void OnContinuePressed() => Close();

    private void OnBackToTitlePressed()
    {
        Close();
        _goToTitle();
    }

    private void Hide()
    {
        _overlay.Visible = false;
        _panel.Visible = false;
    }
}
