// 作弊菜单组件（cheat_menu.tscn 场景根）
using CasualCastle.Adapters.Godot;
using Godot;

public partial class CheatMenuUiController : Control
{
	private ColorRect _overlay;
	private Panel _panel;
	private Button _openButton;
	private CheckBox _fastProductionCheck;
	private Button _closeButton;
	private bool _gameOver;

	public bool IsOpen { get; private set; }

	public override void _Ready()
	{
		_openButton = GetParent().GetNode<HudUiController>("Hud").CheatButton;
		_overlay = GetNode<ColorRect>("CheatOverlay");
		_panel = GetNode<Panel>("CheatPanel");
		_fastProductionCheck = GetNode<CheckBox>("CheatPanel/FastProductionCheck");
		_closeButton = GetNode<Button>("CheatPanel/CloseButton");

		_openButton.Pressed += OnOpenButtonPressed;
		_fastProductionCheck.Toggled += OnFastProductionToggled;
		_closeButton.Pressed += Close;

		_openButton.Visible = DisplaySettingsManager.DevModeEnabled;
		HidePanel();
	}

	public override void _ExitTree()
	{
		_openButton.Pressed -= OnOpenButtonPressed;
		_fastProductionCheck.Toggled -= OnFastProductionToggled;
		_closeButton.Pressed -= Close;
	}

	public void SetGameOver(bool gameOver)
	{
		_gameOver = gameOver;
		if (gameOver)
			Close();
	}

	public bool TryHandleEscape()
	{
		if (!IsOpen)
			return false;
		Close();
		return true;
	}

	private void OnOpenButtonPressed()
	{
		if (IsOpen)
			Close();
		else
			Open();
	}

	private void Open()
	{
		if (IsOpen || _gameOver)
			return;
		IsOpen = true;
		_overlay.Visible = true;
		_panel.Visible = true;
		SyncFromState();
	}

	private void Close()
	{
		if (!IsOpen)
			return;
		IsOpen = false;
		_overlay.Visible = false;
		_panel.Visible = false;
	}

	private void HidePanel()
	{
		_overlay.Visible = false;
		_panel.Visible = false;
	}

	private void SyncFromState()
	{
		_fastProductionCheck.ButtonPressed = CheatState.FastProductionEnabled;
	}

	private void OnFastProductionToggled(bool enabled)
	{
		CheatState.SetFastProduction(enabled);
	}
}
