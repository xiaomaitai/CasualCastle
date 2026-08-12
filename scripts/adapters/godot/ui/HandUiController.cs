// 手牌 UI：展示、选择与拖拽放置
using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Battle;
using CasualCastle.Domain.Building;
using CasualCastle.Domain.Shared;
using Godot;

public sealed class HandUiController
{
	private const float DragThreshold = 8f;

	private readonly Node _owner;
	private readonly Button[] _handButtons = new Button[MaxHandSize];
	private readonly Control.GuiInputEventHandler[] _handGuiInputHandlers = new Control.GuiInputEventHandler[MaxHandSize];
	private readonly Label _placementHintLabel;

	private readonly Button _useItemButton;

	private readonly Hand _hand;
	private readonly Shop _shop;
	private readonly GameManager _gameManager;
	private readonly BuildingSystem _buildingSystem;
	private readonly ShopUiController _shopUi;

	private bool _inputBlocked;
	private bool _dragging;
	private int _dragHandIndex = -1;
	private int _pendingHandIndex = -1;
	private Vector2 _dragStartPosition;
	private int _selectedIndex = -1;
	private const int MaxHandSize = Hand.MaxHandSize;

	public bool IsDragging => _dragging;
	public bool IsPlacementActive => _dragging || _hand.HasSelection;

	public HandUiController(Node owner, CanvasLayer uiRoot, Hand handService, ShopUiController shopUi)
	{
		_owner = owner;
		_hand = handService;
		_shop = AdapterRegistry.Resolve<Shop>();
		_gameManager = AdapterRegistry.Resolve<GameManager>();
		_buildingSystem = AdapterRegistry.Resolve<BuildingSystem>();
		_shopUi = shopUi;
		_placementHintLabel = uiRoot.GetNode<Label>("PlacementHintLabel");

		_useItemButton = new Button { Text = "使用经验魔晶", Visible = false };
		_useItemButton.Position = new Vector2(778f, 740f);
		_useItemButton.Size = new Vector2(180f, 50f);
		_useItemButton.AddThemeFontSizeOverride("font_size", 22);
		uiRoot.AddChild(_useItemButton);
		MedievalUiStyles.ApplyHand(uiRoot, _useItemButton);
		_useItemButton.Pressed += OnUseItemPressed;

		for (int i = 0; i < MaxHandSize; i++)
		{
			_handButtons[i] = uiRoot.GetNode<Button>($"HandPanel/HandSlot{i + 1}");
			int handIndex = i;
			_handButtons[i].Pressed += () => OnHandButtonPressed(handIndex);
			_handGuiInputHandlers[i] = inputEvent => OnHandGuiInput(handIndex, inputEvent);
			_handButtons[i].GuiInput += _handGuiInputHandlers[i];
		}

		_hand.HandChanged += RefreshDisplay;
		_hand.SelectionChanged += OnSelectionChanged;
		RefreshDisplay();
	}

	public void Dispose()
	{
		for (int i = 0; i < MaxHandSize; i++)
			_handButtons[i].GuiInput -= _handGuiInputHandlers[i];

		_hand.HandChanged -= RefreshDisplay;
		_hand.SelectionChanged -= OnSelectionChanged;
		_useItemButton.Pressed -= OnUseItemPressed;
	}

	public void SetInputBlocked(bool blocked)
	{
		_inputBlocked = blocked;
		RefreshDisplay();

		if (_inputBlocked)
		{
			CancelDrag();
			_hand.ClearSelection();
			_gameManager.PlayerCastle.ClearPlacementPreview();
		}
	}

	public void Process()
	{
		UpdatePendingDrag();
		UpdatePlacementPreview();
		UpdateSellDropHighlight();
	}

	private void UpdateSellDropHighlight()
	{
		bool overPanel = _dragging && _shopUi != null && _shopUi.IsOpen
			&& _shopUi.IsPointInPanel(_owner.GetViewport().GetMousePosition());
		_shopUi?.SetSellHighlight(overPanel);
	}

	public bool TryHandleEscape()
	{
		if (CancelDrag())
			return true;

		return ClearSelection();
	}

	public bool HandleInput(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseButton && !mouseButton.Pressed)
		{
			if (mouseButton.ButtonIndex == MouseButton.Left)
			{
				if (_dragging)
				{
					bool placed = TryCompleteDrag(mouseButton.GlobalPosition);
					CancelDrag(!placed);
					return true;
				}

				if (_pendingHandIndex >= 0)
				{
					_pendingHandIndex = -1;
					return false;
				}
			}

			if (mouseButton.ButtonIndex == MouseButton.Right && _dragging)
			{
				CancelDrag();
				return true;
			}
		}

		if (@event is InputEventMouseButton pressedMouseButton && pressedMouseButton.Pressed)
			return HandleMouseInput(pressedMouseButton);

		return false;
	}

	public bool CancelDrag(bool playFeedback = true)
	{
		_shopUi?.SetSellHighlight(false);
		if (!_dragging && _pendingHandIndex < 0)
			return false;

		_dragging = false;
		_dragHandIndex = -1;
		_pendingHandIndex = -1;
		_gameManager.PlayerCastle.ClearPlacementPreview();
		RefreshHighlight();
		UpdatePlacementHint();
		if (playFeedback)
			Sfx.Play(SfxEvents.PlaceCancel);
		return true;
	}

	private bool HandleMouseInput(InputEventMouseButton mouseButton)
	{
		if (mouseButton.ButtonIndex == MouseButton.Right)
			return ClearSelection();

		if (mouseButton.ButtonIndex != MouseButton.Left)
			return false;

		if (_inputBlocked || _dragging || _gameManager.CurrentState == GameManager.GameState.GameOver)
			return false;

		if (!_hand.HasSelection || IsInteractiveUiHovered())
			return false;

		Castle playerCastle = _gameManager.PlayerCastle;
		if (playerCastle == null)
			return false;

		if (!playerCastle.TryGetGridFromScreenPoint(mouseButton.GlobalPosition, out int gridX, out int gridY))
			return false;

		_hand.TryPlaceSelected(gridX, gridY);
		return true;
	}

	private bool ClearSelection()
	{
		if (!_hand.HasSelection)
			return false;

		_hand.ClearSelection();
		return true;
	}

	private bool IsInteractiveUiHovered()
	{
		Control hoveredControl = _owner.GetViewport().GuiGetHoveredControl();
		return hoveredControl != null && hoveredControl.MouseFilter != Control.MouseFilterEnum.Ignore;
	}

	private void OnHandButtonPressed(int handIndex)
	{
		if (_inputBlocked)
			return;

		_hand.SelectCard(handIndex);
	}

	private void OnHandGuiInput(int handIndex, InputEvent @event)
	{
		if (@event is not InputEventMouseButton mouseButton
			|| !mouseButton.Pressed
			|| mouseButton.ButtonIndex != MouseButton.Left)
			return;

		if (_inputBlocked || _gameManager.CurrentState == GameManager.GameState.GameOver)
			return;

		if (handIndex >= _hand.Cards.Count)
			return;

		_pendingHandIndex = handIndex;
		_dragStartPosition = mouseButton.GlobalPosition;
	}

	private void UpdatePendingDrag()
	{
		if (_pendingHandIndex < 0 || _dragging)
			return;

		if (!Input.IsMouseButtonPressed(MouseButton.Left))
			return;

		Vector2 mouseGlobal = _owner.GetViewport().GetMousePosition();
		if (mouseGlobal.DistanceTo(_dragStartPosition) < DragThreshold)
			return;

		_dragging = true;
		_dragHandIndex = _pendingHandIndex;
		_pendingHandIndex = -1;
		RefreshHighlight();
		UpdatePlacementHint();
	}

	private bool TryCompleteDrag(Vector2 screenPosition)
	{
		Castle playerCastle = _gameManager.PlayerCastle;
		if (playerCastle == null || _dragHandIndex < 0)
			return false;

		if (_shopUi != null && _shopUi.IsOpen && _shopUi.IsPointInPanel(screenPosition))
			return _shop.TrySellHandCard(_dragHandIndex);

		if (!playerCastle.TryGetGridFromScreenPoint(screenPosition, out int gridX, out int gridY))
			return false;

		return _hand.TryPlaceAtIndex(_dragHandIndex, gridX, gridY);
	}

	private void RefreshDisplay()
	{
		for (int i = 0; i < MaxHandSize; i++)
		{
			Button button = _handButtons[i];

			if (i < _hand.Cards.Count)
			{
				CardData card = _hand.Cards[i];
				button.Visible = true;
				button.Text = $"[{i + 1}]\n{card.Name}\n金币 {card.Cost}";
				button.Disabled = _inputBlocked;
			}
			else
			{
				button.Visible = false;
				button.Text = "";
				button.Disabled = true;
			}
		}

		RefreshHighlight();
		_useItemButton.Visible = CanUseSelectedCrystal();
	}

	private void OnSelectionChanged(int selectedIndex)
	{
		_selectedIndex = selectedIndex;
		RefreshHighlight();
		UpdatePlacementHint();
		_useItemButton.Visible = CanUseSelectedCrystal();
	}

	private bool CanUseSelectedCrystal()
	{
		if (_inputBlocked || !_hand.HasSelection)
			return false;
		return _hand.SelectedCard?.ItemType == ShopItemTypes.DungeonCrystal;
	}

	private void OnUseItemPressed()
	{
		CardData card = _hand.SelectedCard;
		if (card == null || !_hand.TryRemoveCardById(card.Id))
			return;
		GameManager.Get<KingExperienceService>()?.OnCrystalUsed();
		Sfx.Play(SfxEvents.Upgrade);
		_useItemButton.Visible = false;
	}

	private void UpdatePlacementHint()
	{
		_placementHintLabel.Visible = (_selectedIndex >= 0 || _dragging) && !_inputBlocked && IsPlacementCardPlaceable();
	}

	private void RefreshHighlight()
	{
		for (int i = 0; i < MaxHandSize; i++)
		{
			bool highlighted = i == _selectedIndex || (_dragging && i == _dragHandIndex);
			MedievalUiStyles.ApplyCardButton(_handButtons[i], highlighted, i < _hand.Cards.Count);
			_handButtons[i].Modulate = Colors.White;
		}
	}

	private void UpdatePlacementPreview()
	{
		Castle playerCastle = _gameManager.PlayerCastle;
		if (playerCastle == null)
			return;

		bool showPreview = !_inputBlocked
			&& (_dragging || _hand.HasSelection)
			&& IsPlacementCardPlaceable();
		if (!showPreview)
		{
			playerCastle.ClearPlacementPreview();
			return;
		}

		Vector2 mouseScreen = _owner.GetViewport().GetMousePosition();
		if (!playerCastle.TryGetGridFromScreenPoint(mouseScreen, out int gridX, out int gridY))
		{
			playerCastle.ClearPlacementPreview();
			return;
		}

		string buildingType = GetPreviewBuildingType();
		bool valid = _buildingSystem.CanPlace(playerCastle, buildingType, gridX, gridY);
		playerCastle.SetPlacementPreview(true, gridX, gridY, valid, buildingType);
	}

	private bool IsPlacementCardPlaceable()
	{
		CardData card = _dragging && _dragHandIndex >= 0 && _dragHandIndex < _hand.Cards.Count
			? _hand.Cards[_dragHandIndex]
			: _hand.SelectedCard;
		return card != null && card.IsPlaceable;
	}

	private string GetPreviewBuildingType()
	{
		if (_dragging && _dragHandIndex >= 0 && _dragHandIndex < _hand.Cards.Count)
			return _hand.Cards[_dragHandIndex].BuildingType;

		if (_hand.HasSelection)
			return _hand.SelectedCard.BuildingType;

		return BuildingTypeIds.Barracks;
	}
}
