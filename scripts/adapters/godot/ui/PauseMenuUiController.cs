// 暂停菜单组件（pause_menu.tscn 场景根）
using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Shared;
using Godot;
using System;

public partial class PauseMenuUiController : Control
{
	private ColorRect _overlay;
	private Panel _panel;
	private Button _menuButton;
	private Button _continueButton;
	private Button _saveButton;
	private Button _backToTitleButton;
	private Button _settingsButton;

	public Action GoToTitleAction { get; set; }
	public Action OpenSettingsAction { get; set; }

	private bool _gameOver;

	public bool IsOpen { get; private set; }
	public event Action<bool> OpenChanged;

	public override void _Ready()
	{
		_overlay = GetNode<ColorRect>("PauseOverlay");
		_panel = GetNode<Panel>("PausePanel");
		_menuButton = GetParent().GetNode<HudUiController>("Hud").MenuButton;
		_continueButton = GetNode<Button>("PausePanel/ContinueButton");
		_saveButton = GetNode<Button>("PausePanel/SaveButton");
		_backToTitleButton = GetNode<Button>("PausePanel/BackToTitleButton");
		_settingsButton = GetNode<Button>("PausePanel/SettingsButton");

		_menuButton.Pressed += OnMenuButtonPressed;
		_continueButton.Pressed += OnContinuePressed;
		_saveButton.Pressed += OnSavePressed;
		_backToTitleButton.Pressed += OnBackToTitlePressed;
		_settingsButton.Pressed += OnSettingsPressed;

		HidePanel();
	}

	public override void _ExitTree()
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
		HidePanel();
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
		GameManager.Get<IGameSessionService>().SaveGame(0);
	}

	private void OnBackToTitlePressed()
	{
		Close();
		GoToTitleAction?.Invoke();
	}

	private void OnSettingsPressed()
	{
		OpenSettingsAction?.Invoke();
	}

	private void HidePanel()
	{
		_overlay.Visible = false;
		_panel.Visible = false;
	}
}
