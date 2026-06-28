using CasualCastle.Adapters.Godot;
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
	private readonly ButtonGroup _toolGroup;

	private ToolMode _mode = ToolMode.None;
	private bool _gameOver;
	private bool _inputBlocked;
	private bool _repairBlockedHover;

	public bool IsToolActive => _mode != ToolMode.None;

	public BuildingManageUiController(Node owner, CanvasLayer uiRoot, ButtonGroup toolGroup = null)
	{
		_owner = owner;
		_pauseButton = uiRoot.GetNode<Button>("BuildingPauseToolButton");
		_repairButton = uiRoot.GetNode<Button>("BuildingRepairToolButton");

		Node mainGame = owner.GetParent();
		_playerCastle = mainGame.GetNode<Castle>("Battlefield/PlayerSide/PlayerCastle");

		_toolGroup = toolGroup ?? new ButtonGroup();
		_pauseButton.ButtonGroup = _toolGroup;
		_repairButton.ButtonGroup = _toolGroup;
		_pauseButton.ToggleMode = true;
		_repairButton.ToggleMode = true;
		_pauseButton.Toggled += OnPauseToggled;
		_repairButton.Toggled += OnRepairToggled;

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
		_pauseButton.Toggled -= OnPauseToggled;
		_repairButton.Toggled -= OnRepairToggled;
		ResetCursor();
	}

	public void Process()
	{
		if (_mode != ToolMode.Repair || _gameOver || _inputBlocked)
			return;

		UpdateRepairCursor();
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

		if (@event is not InputEventMouseButton mouseButton || !mouseButton.Pressed)
			return false;

		if (mouseButton.ButtonIndex == MouseButton.Right)
		{
			SetMode(ToolMode.None);
			return true;
		}

		if (mouseButton.ButtonIndex != MouseButton.Left)
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

	private void OnPauseToggled(bool pressed)
	{
		if (pressed)
			SetMode(ToolMode.Pause);
		else if (_mode == ToolMode.Pause)
			SetMode(ToolMode.None);
	}

	private void OnRepairToggled(bool pressed)
	{
		if (pressed)
			SetMode(ToolMode.Repair);
		else if (_mode == ToolMode.Repair)
			SetMode(ToolMode.None);
	}

	private void SetMode(ToolMode mode)
	{
		_mode = mode;
		_repairBlockedHover = false;
		UpdateButtonVisuals();
		UpdateCursor();
	}

	private void UpdateButtonVisuals()
	{
		_pauseButton.SetPressedNoSignal(_mode == ToolMode.Pause);
		_repairButton.SetPressedNoSignal(_mode == ToolMode.Repair);
	}

	private void UpdateCursor()
	{
		Texture2D cursor = _mode switch
		{
			ToolMode.Pause => BuildingIcons.Pause,
			ToolMode.Repair => BuildingIcons.Repair,
			_ => null,
		};

		if (cursor != null)
			Input.SetCustomMouseCursor(cursor, Input.CursorShape.Arrow, BuildingIcons.CursorHotspot);
		else
			Input.SetCustomMouseCursor(null);
	}

	private void UpdateRepairCursor()
	{
		bool blocked = false;
		Vector2 mouse = _owner.GetViewport().GetMousePosition();
		if (_playerCastle.TryGetBuildingAtGlobalPoint(mouse, out Building building)
			&& CanManage(building)
			&& building.IsDamaged
			&& !building.CanRepair())
		{
			blocked = true;
		}

		if (blocked == _repairBlockedHover)
			return;

		_repairBlockedHover = blocked;
		Texture2D cursor = blocked ? BuildingIcons.RepairBlocked : BuildingIcons.Repair;
		Input.SetCustomMouseCursor(cursor, Input.CursorShape.Arrow, BuildingIcons.CursorHotspot);
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

	private void TogglePause(Building building)
	{
		if (building.IsDestroyed)
			return;

		building.SetManuallyPaused(!building.IsManuallyPaused);
	}

	private void TryRepair(Building building)
	{
		if (!building.IsDamaged || AdapterRegistry.Resolve<ShopSystem>() == null)
			return;

		AdapterRegistry.Resolve<ShopSystem>().TryRepairBuilding(building);
	}
}
