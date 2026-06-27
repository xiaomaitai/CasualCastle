using Godot;

public sealed class FusionProhibitUiController
{
	private readonly Button _button;
	private readonly Castle _playerCastle;
	private readonly ButtonGroup _toolGroup;

	private bool _active;
	private bool _gameOver;
	private bool _inputBlocked;

	public bool IsToolActive => _active;

	public FusionProhibitUiController(Node owner, CanvasLayer uiRoot, ButtonGroup toolGroup)
	{
		_toolGroup = toolGroup;
		_button = uiRoot.GetNode<Button>("FusionProhibitToolButton");

		Node mainGame = owner.GetParent();
		_playerCastle = mainGame.GetNode<Castle>("Battlefield/PlayerSide/PlayerCastle");

		_button.ButtonGroup = _toolGroup;
		_button.ToggleMode = true;
		_button.Toggled += OnToggled;
		_button.Icon = BuildingIcons.FusionProhibit;
		_button.ExpandIcon = true;
		_button.Text = "";
		_button.TooltipText = "禁止融合";

		UpdateButtonVisual();
	}

	public void Dispose()
	{
		_button.Toggled -= OnToggled;
		ResetCursor();
	}

	public void SetGameOver(bool gameOver)
	{
		_gameOver = gameOver;
		if (_gameOver)
			SetActive(false);
	}

	public void SetInputBlocked(bool blocked)
	{
		_inputBlocked = blocked;
		if (_inputBlocked)
			SetActive(false);
	}

	public bool TryHandleEscape()
	{
		if (!_active)
			return false;

		SetActive(false);
		return true;
	}

	public bool HandleInput(InputEvent @event)
	{
		if (!_active || _gameOver || _inputBlocked)
			return false;

		if (@event is not InputEventMouseButton mouseButton || !mouseButton.Pressed)
			return false;

		if (mouseButton.ButtonIndex == MouseButton.Right)
		{
			SetActive(false);
			return true;
		}

		if (mouseButton.ButtonIndex != MouseButton.Left)
			return false;

		if (!_playerCastle.TryGetBuildingAtGlobalPoint(mouseButton.GlobalPosition, out Building building))
			return false;

		if (!CanManage(building))
			return false;

		building.SetFusionProhibited(!building.IsFusionProhibited);
		return true;
	}

	private void OnToggled(bool pressed)
	{
		if (pressed)
			SetActive(true);
		else if (_active)
			SetActive(false);
	}

	private void SetActive(bool active)
	{
		_active = active;
		UpdateButtonVisual();
		UpdateCursor();
	}

	private void UpdateButtonVisual()
	{
		_button.SetPressedNoSignal(_active);
	}

	private void UpdateCursor()
	{
		if (_active)
			Input.SetCustomMouseCursor(BuildingIcons.FusionProhibit, Input.CursorShape.Arrow, BuildingIcons.CursorHotspot);
		else
			Input.SetCustomMouseCursor(null);
	}

	private static void ResetCursor()
	{
		Input.SetCustomMouseCursor(null);
	}

	private static bool CanManage(Building building)
	{
		if (building == null || building.GetCastle() == null)
			return false;

		if (!building.GetCastle().IsPlayerCastle)
			return false;

		return !BuildingSystem.IsCoreBuilding(building.TypeId);
	}
}
