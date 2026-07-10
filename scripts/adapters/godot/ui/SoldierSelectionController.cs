using Godot;

public sealed class SoldierSelectionController
{
	private readonly Node _owner;
	private readonly Node2D _battlefield;
	private SoldierLogic _selected;
	private bool _inputBlocked;

	public SoldierSelectionController(Node owner)
	{
		_owner = owner;
		_battlefield = owner.GetParent().GetNode<Node2D>("Battlefield");
	}

	public void SetInputBlocked(bool blocked)
	{
		_inputBlocked = blocked;
		if (blocked)
			ClearSelection();
	}

	public void Process()
	{
		if (_selected != null && (!GodotObject.IsInstanceValid(_selected) || !_selected.IsAlive))
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

		Control hoveredControl = _owner.GetViewport().GuiGetHoveredControl();
		if (hoveredControl != null && hoveredControl.MouseFilter != Control.MouseFilterEnum.Ignore)
			return false;

		SoldierLogic picked = PickSoldier(mouseButton.Position);
		if (picked == _selected)
			return true;

		ClearSelection();
		if (picked != null)
		{
			_selected = picked;
			_selected.SetSelected(true);
		}
		return true;
	}

	private SoldierLogic PickSoldier(Vector2 globalPoint)
	{
		SoldierLogic closest = null;
		float closestDistance = 40f;

		foreach (Node child in _battlefield.GetChildren())
		{
			Area2D area = child as Area2D;
			if (area == null)
				continue;

			SoldierLogic logic = area.GetNodeOrNull<SoldierLogic>("Logic");
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

	private void ClearSelection()
	{
		if (_selected != null && GodotObject.IsInstanceValid(_selected))
			_selected.SetSelected(false);
		_selected = null;
	}
}
