// 单位选择控制：点击拾取与属性面板联动
using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Battle;
using Godot;

public partial class UnitSelectionController : Node
{
	private Node2D _battlefield;
	private Castle _playerCastle;
	private Castle _enemyCastle;
	private UnitLogic _selectedUnit;
	private Building _selectedBuilding;
	private TrapRangeView _trapRange;
	private bool _inputBlocked;

	public UnitLogic SelectedUnit => _selectedUnit;
	public Building SelectedBuilding => _selectedBuilding;

	public override void _Ready()
	{
		_battlefield = GetParent().GetParent().GetNode<Node2D>("Battlefield");
		_playerCastle = _battlefield.GetNodeOrNull<Castle>("PlayerSide/PlayerCastle");
		_enemyCastle = _battlefield.GetNodeOrNull<Castle>("EnemySide/EnemyCastle");
	}

	public void SetInputBlocked(bool blocked)
	{
		_inputBlocked = blocked;
		if (blocked)
			ClearSelection();
	}

	public override void _Process(double _)
	{
		if (_selectedUnit != null && (!GodotObject.IsInstanceValid(_selectedUnit) || !_selectedUnit.IsAlive))
			ClearSelection();
		if (_selectedBuilding != null && (!GodotObject.IsInstanceValid(_selectedBuilding) || _selectedBuilding.IsDestroyed))
			ClearSelection();
	}

	public bool HandleInput(InputEvent @event)
	{
		if (_inputBlocked)
			return false;

		if (@event is not InputEventMouseButton mouseButton
			|| mouseButton.ButtonIndex != MouseButton.Left
			|| !mouseButton.Pressed)
			return false;

		Control hoveredControl = GetViewport().GuiGetHoveredControl();
		if (hoveredControl != null && hoveredControl.MouseFilter != Control.MouseFilterEnum.Ignore)
			return false;

		Vector2 worldPoint = GameCoordinatesAdapter.ScreenToWorld(GetViewport(), mouseButton.GlobalPosition);
		UnitLogic picked = PickUnit(worldPoint);
		if (picked != null)
		{
			if (picked == _selectedUnit)
				return true;
			ClearSelection();
			_selectedUnit = picked;
			_selectedUnit.SetSelected(true);
			Sfx.Play(SfxEvents.UnitSelect);
			return true;
		}

		Building pickedBuilding = PickBuilding(mouseButton.GlobalPosition);
		if (pickedBuilding != null)
		{
			if (pickedBuilding == _selectedBuilding)
				return true;
			ClearSelection();
			_selectedBuilding = pickedBuilding;
			ShowTrapRange(pickedBuilding);
			Sfx.Play(SfxEvents.BuildingSelect);
			return true;
		}

		ClearSelection();
		return false;
	}

	private UnitLogic PickUnit(Vector2 globalPoint)
	{
		UnitLogic closest = null;
		float closestDistance = 40f;

		foreach (Node child in _battlefield.GetChildren())
		{
			Area2D area = child as Area2D;
			if (area == null)
				continue;

			UnitLogic logic = area.GetNodeOrNull<UnitLogic>("Logic");
			if (logic == null || !logic.IsAlive)
				continue;

			float distance = area.GlobalPosition.DistanceTo(globalPoint);
			if (distance < closestDistance)
			{
				closestDistance = distance;
				closest = logic;
			}
		}

		return closest;
	}

	private Building PickBuilding(Vector2 screenPoint)
	{
		if (_playerCastle != null && _playerCastle.TryGetBuildingAtScreenPoint(screenPoint, out Building building))
			return building;
		if (_enemyCastle != null && _enemyCastle.TryGetBuildingAtScreenPoint(screenPoint, out building))
			return building;
		return null;
	}

	private void ShowTrapRange(Building building)
	{
		HideTrapRange();
		if (!TrapRules.IsTrap(building.TypeId))
			return;

		float rangeUnits = TrapRules.GetConfig(building.TypeId).Range;
		_trapRange = new TrapRangeView();
		_trapRange.Configure(GameCoordinatesAdapter.GameUnitsToPixels(rangeUnits));
		_battlefield.AddChild(_trapRange);
		_trapRange.GlobalPosition = building.GetCastle().ToGlobal(building.GetMainCellLocalPosition());
	}

	private void HideTrapRange()
	{
		if (_trapRange == null)
			return;
		_trapRange.QueueFree();
		_trapRange = null;
	}

	private void ClearSelection()
	{
		if (_selectedUnit != null && GodotObject.IsInstanceValid(_selectedUnit))
			_selectedUnit.SetSelected(false);
		_selectedUnit = null;
		_selectedBuilding = null;
		HideTrapRange();
	}
}