using CasualCastle.Adapters.Godot;
using Godot;

public sealed class CheatMenuUiController
{
	private readonly ColorRect _overlay;
	private readonly Panel _panel;
	private readonly Button _openButton;
	private readonly CheckBox _fastProductionCheck;
	private readonly Button _closeButton;
	private bool _gameOver;

	public bool IsOpen { get; private set; }

	public CheatMenuUiController(CanvasLayer uiRoot)
	{
		_openButton = uiRoot.GetNode<Button>("PhasePanel/CheatButton");
		_overlay = uiRoot.GetNode<ColorRect>("CheatOverlay");
		_panel = uiRoot.GetNode<Panel>("CheatPanel");
		_fastProductionCheck = uiRoot.GetNode<CheckBox>("CheatPanel/FastProductionCheck");
		_closeButton = uiRoot.GetNode<Button>("CheatPanel/CloseButton");

		_openButton.Pressed += OnOpenButtonPressed;
		_fastProductionCheck.Toggled += OnFastProductionToggled;
		_closeButton.Pressed += Close;

		_openButton.Visible = DisplaySettingsManager.DevModeEnabled;
		Hide();
	}

	public void Dispose()
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

	private void Hide()
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
