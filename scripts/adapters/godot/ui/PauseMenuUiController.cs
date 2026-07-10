using CasualCastle.Adapters.Godot;
using Godot;
using System;

public sealed class PauseMenuUiController
{
    private readonly ColorRect _overlay;
    private readonly Panel _panel;
    private readonly Button _menuButton;
    private readonly Button _continueButton;
    private readonly Button _saveButton;
    private readonly Button _backToTitleButton;
    private readonly Button _settingsButton;
    private readonly Action _goToTitle;
    private readonly Action _openSettings;

    private bool _gameOver;

    public bool IsOpen { get; private set; }
    public event Action<bool> OpenChanged;

    public PauseMenuUiController(CanvasLayer uiRoot, Action goToTitle, Action openSettings)
    {
        _overlay = uiRoot.GetNode<ColorRect>("PauseOverlay");
        _panel = uiRoot.GetNode<Panel>("PausePanel");
        _menuButton = uiRoot.GetNode<Button>("MenuButton");
        _continueButton = uiRoot.GetNode<Button>("PausePanel/ContinueButton");
        _backToTitleButton = uiRoot.GetNode<Button>("PausePanel/BackToTitleButton");
        _settingsButton = uiRoot.GetNode<Button>("PausePanel/SettingsButton");
        _goToTitle = goToTitle;
        _openSettings = openSettings;

        _saveButton = new Button();
        _saveButton.Name = "SaveButton";
        _saveButton.Text = "保存游戏";
        _saveButton.OffsetLeft = 138;
        _saveButton.OffsetTop = 438;
        _saveButton.OffsetRight = 438;
        _saveButton.OffsetBottom = 510;
        _panel.AddChild(_saveButton);

        _menuButton.Pressed += OnMenuButtonPressed;
        _continueButton.Pressed += OnContinuePressed;
        _saveButton.Pressed += OnSavePressed;
        _backToTitleButton.Pressed += OnBackToTitlePressed;
        _settingsButton.Pressed += OnSettingsPressed;

        Hide();
    }

    public void Dispose()
    {
        _menuButton.Pressed -= OnMenuButtonPressed;
        _continueButton.Pressed -= OnContinuePressed;
        _saveButton.Pressed -= OnSavePressed;
        _backToTitleButton.Pressed -= OnBackToTitlePressed;
        _settingsButton.Pressed -= OnSettingsPressed;
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
        if (IsOpen || _gameOver || AdapterRegistry.Resolve<GameManager>()?.CurrentState != GameManager.GameState.Playing)
            return false;

        IsOpen = true;
        _overlay.Visible = true;
        _panel.Visible = true;
        AdapterRegistry.Resolve<GameManager>().SetPaused(true);
        OpenChanged?.Invoke(true);
        return true;
    }

    public bool Close()
    {
        if (!IsOpen)
            return false;

        IsOpen = false;
        Hide();
        AdapterRegistry.Resolve<GameManager>()?.SetPaused(false);
        OpenChanged?.Invoke(false);
        return true;
    }

    public bool Toggle()
    {
        return IsOpen ? Close() : Open();
    }

    private void OnMenuButtonPressed() => Open();

    private void OnContinuePressed() => Close();

    private void OnSavePressed()
    {
        AdapterRegistry.Resolve<GameManager>().SaveGame(0);
    }

    private void OnBackToTitlePressed()
    {
        Close();
        _goToTitle();
    }

    private void OnSettingsPressed()
    {
        _openSettings?.Invoke();
    }

    private void Hide()
    {
        _overlay.Visible = false;
        _panel.Visible = false;
    }
}
