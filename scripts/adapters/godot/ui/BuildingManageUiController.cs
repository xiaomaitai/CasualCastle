// 建筑管理 UI：暂停、修复与禁止组合
using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Building;
using Godot;

public partial class BuildingManageUiController : Node
{
	private enum ToolMode
	{
		None,
		Pause,
		Repair,
	}

	private Button _pauseButton;
	private Button _repairButton;
	private Castle _playerCastle;
	private ButtonGroup _toolGroup;

	public HudUiController Hud { get; set; }
	public ButtonGroup ToolGroup { get; set; }

	private ToolMode _mode = ToolMode.None;
	private bool _gameOver;
	private bool _inputBlocked;
	private bool _repairBlockedHover;

	public bool IsToolActive => _mode != ToolMode.None;

	public override void _Ready()
	{
		_pauseButton = Hud.BuildingPauseToolButton;
		_repairButton = Hud.BuildingRepairToolButton;

		_playerCastle = GetParent().GetParent().GetNode<Castle>("Battlefield/PlayerSide/PlayerCastle");

		_toolGroup = ToolGroup ?? new ButtonGroup();
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

	public override void _ExitTree()
	{
		_pauseButton.Toggled -= OnPauseToggled;
		_repairButton.Toggled -= OnRepairToggled;
		ResetCursor();
	}

	public override void _Process(double _)
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

		if (!_playerCastle.TryGetBuildingAtScreenPoint(mouseButton.GlobalPosition, out Building building))
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
		Vector2 mouse = GetViewport().GetMousePosition();
		if (_playerCastle.TryGetBuildingAtScreenPoint(mouse, out Building building)
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

		return !AdapterRegistry.Resolve<BuildingSystem>().IsCoreBuilding(building.TypeId);
	}

	private void TogglePause(Building building)
	{
		if (building.IsDestroyed)
			return;

		building.SetManuallyPaused(!building.IsManuallyPaused);
	}

	private void TryRepair(Building building)
	{
		if (!building.IsDamaged)
			return;

		if (building.TryRepair())
			Sfx.Play(SfxEvents.Repair);
	}
}