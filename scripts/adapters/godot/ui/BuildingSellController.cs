// 建筑卖出拖拽：将玩家建筑拖到商店面板卖出
using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Building;
using Godot;

public partial class BuildingSellController : Node
{
	private const float DragThreshold = 12f;

	private Castle _playerCastle;
	private Shop _shop;
	private ShopUiController _shopUi;

	public Castle PlayerCastle { get; set; }
	public Shop Shop { get; set; }
	public ShopUiController ShopUi { get; set; }

	private Building _pressedBuilding;
	private Vector2 _pressStartPosition;
	private bool _dragging;
	private bool _inputBlocked;

	public bool IsDragging => _dragging;

	public override void _Ready()
	{
		_playerCastle = PlayerCastle;
		_shop = Shop;
		_shopUi = ShopUi;
	}

	public void SetInputBlocked(bool blocked)
	{
		_inputBlocked = blocked;
		if (_inputBlocked)
			CancelDrag();
	}

	public override void _Process(double _)
	{
		if (_dragging && _shopUi != null)
		{
			bool overPanel = _shopUi.IsOpen && _shopUi.IsPointInPanel(GetViewport().GetMousePosition());
			_shopUi.SetSellHighlight(overPanel);
			return;
		}

		if (_pressedBuilding == null || !Input.IsMouseButtonPressed(MouseButton.Left))
			return;

		Vector2 mousePosition = GetViewport().GetMousePosition();
		if (mousePosition.DistanceTo(_pressStartPosition) < DragThreshold)
			return;

		_dragging = true;
	}

	public bool HandleInput(InputEvent @event)
	{
		if (_inputBlocked || @event is not InputEventMouseButton mouseButton)
			return false;

		if (mouseButton.Pressed && mouseButton.ButtonIndex == MouseButton.Left)
			return TryBeginDrag(mouseButton.GlobalPosition);

		if (mouseButton.Pressed && mouseButton.ButtonIndex == MouseButton.Right && _dragging)
		{
			CancelDrag();
			return true;
		}

		if (!mouseButton.Pressed && mouseButton.ButtonIndex == MouseButton.Left && _dragging)
		{
			bool sold = TryCompleteDrag(mouseButton.GlobalPosition);
			CancelDrag();
			return true;
		}

		return false;
	}

	private bool TryBeginDrag(Vector2 screenPosition)
	{
		if (_dragging || _shopUi == null || !_shopUi.IsOpen)
			return false;

		if (!_playerCastle.TryGetBuildingAtScreenPoint(screenPosition, out Building building) || !CanSell(building))
			return false;

		_pressedBuilding = building;
		_pressStartPosition = screenPosition;
		return false;
	}

	private bool TryCompleteDrag(Vector2 screenPosition)
	{
		if (_pressedBuilding == null || !GodotObject.IsInstanceValid(_pressedBuilding))
			return false;

		if (_shopUi == null || !_shopUi.IsOpen || !_shopUi.IsPointInPanel(screenPosition))
			return false;

		return SellBuilding(_pressedBuilding);
	}

	private bool SellBuilding(Building building)
	{
		Castle castle = building.GetCastle();
		if (castle == null || !castle.IsPlayerCastle)
			return false;

		int price = SellRules.GetSellPrice(GameManager.Get<IBuildingRepository>().GetValue(building.TypeId));
		_shop.AddGold(price);
		castle.ReleaseBuildingFootprint(building);
		building.GetParent()?.RemoveChild(building);
		building.QueueFree();
		GameManager.Get<AdjacencyService>()?.RefreshCastle(castle.GetBuildingStates());
		return true;
	}

	private void CancelDrag()
	{
		_dragging = false;
		_pressedBuilding = null;
		_shopUi?.SetSellHighlight(false);
	}

	private bool CanSell(Building building)
	{
		if (building == null || !GodotObject.IsInstanceValid(building) || building.IsDestroyed)
			return false;
		if (building.IsRitualInProgress)
			return false;
		if (!building.IsPlayerOwned)
			return false;
		return !GameManager.Get<IBuildingRepository>().IsCoreBuilding(building.TypeId);
	}
}