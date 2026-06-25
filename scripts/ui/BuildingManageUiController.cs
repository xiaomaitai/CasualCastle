using Godot;

public sealed class BuildingManageUiController
{
	private enum ToolMode
	{
		None,
		Pause,
		Repair,
	}

	private readonly Node _owner;
	private readonly Button _pauseButton;
	private readonly Button _repairButton;
	private readonly Castle _playerCastle;

	private ToolMode _mode = ToolMode.None;
	private bool _gameOver;
	private bool _inputBlocked;

	public bool IsToolActive => _mode != ToolMode.None;

	public BuildingManageUiController(Node owner, CanvasLayer uiRoot)
	{
		_owner = owner;
		_pauseButton = uiRoot.GetNode<Button>("BuildingPauseToolButton");
		_repairButton = uiRoot.GetNode<Button>("BuildingRepairToolButton");

		Node mainGame = owner.GetParent();
		_playerCastle = mainGame.GetNode<Castle>("Battlefield/PlayerSide/PlayerCastle");

		_pauseButton.Pressed += OnPauseToolPressed;
		_repairButton.Pressed += OnRepairToolPressed;

		_pauseButton.ToggleMode = true;
		_repairButton.ToggleMode = true;
		_pauseButton.Icon = BuildingIcons.Pause;
		_repairButton.Icon = BuildingIcons.Repair;
		_pauseButton.ExpandIcon = true;
		_repairButton.ExpandIcon = true;
		_pauseButton.Text = "";
		_repairButton.Text = "";
		_pauseButton.TooltipText = "暂停建筑";
		_repairButton.TooltipText = "修复建筑（仅夜晚）";

		UpdateButtonVisuals();
	}

	public void Dispose()
	{
		_pauseButton.Pressed -= OnPauseToolPressed;
		_repairButton.Pressed -= OnRepairToolPressed;
	}

	public void SetGameOver(bool gameOver)
	{
		_gameOver = gameOver;
		if (_gameOver)
			SetMode(ToolMode.None);
	}

	public void SetInputBlocked(bool blocked)
	{
		_inputBlocked = blocked;
		if (_inputBlocked)
			SetMode(ToolMode.None);
	}

	public bool TryHandleEscape()
	{
		if (_mode == ToolMode.None)
			return false;

		SetMode(ToolMode.None);
		return true;
	}

	public bool HandleInput(InputEvent @event)
	{
		if (_mode == ToolMode.None || _gameOver || _inputBlocked)
			return false;

		if (@event is not InputEventMouseButton mouseButton
			|| !mouseButton.Pressed
			|| mouseButton.ButtonIndex != MouseButton.Left)
			return false;

		if (!_playerCastle.TryGetBuildingAtGlobalPoint(mouseButton.GlobalPosition, out Building building))
			return false;

		if (!CanManage(building))
			return false;

		if (_mode == ToolMode.Pause)
			TogglePause(building);
		else if (_mode == ToolMode.Repair)
			TryRepair(building);

		return true;
	}

	private void OnPauseToolPressed()
	{
		SetMode(_mode == ToolMode.Pause ? ToolMode.None : ToolMode.Pause);
	}

	private void OnRepairToolPressed()
	{
		SetMode(_mode == ToolMode.Repair ? ToolMode.None : ToolMode.Repair);
	}

	private void SetMode(ToolMode mode)
	{
		_mode = mode;
		UpdateButtonVisuals();
	}

	private void UpdateButtonVisuals()
	{
		_pauseButton.ButtonPressed = _mode == ToolMode.Pause;
		_repairButton.ButtonPressed = _mode == ToolMode.Repair;
	}

	private static bool CanManage(Building building)
	{
		if (building == null || building.GetCastle() == null)
			return false;

		if (!building.GetCastle().IsPlayerCastle)
			return false;

		return !BuildingSystem.IsCoreBuilding(building.TypeId);
	}

	private void TogglePause(Building building)
	{
		if (building.IsDestroyed)
			return;

		building.SetManuallyPaused(!building.IsManuallyPaused);
	}

	private void TryRepair(Building building)
	{
		if (!building.IsDamaged || ShopSystem.Instance == null)
			return;

		ShopSystem.Instance.TryRepairBuilding(building);
	}
}
