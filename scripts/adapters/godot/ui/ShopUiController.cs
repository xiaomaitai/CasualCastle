// 商店 UI：购买与直接放置
using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Building;
using CasualCastle.Domain.Shared;
using Godot;
using System;

public sealed class ShopUiController
{
	private const string ShopIconPath = "res://assets/art/ui/shop_goods_pile_194.png";
	private readonly Node _owner;
	private readonly Button _shopButton;
	private readonly Panel _shopButtonCircle;
	private readonly Panel _panel;
	private readonly Label _shopGoldLabel;
	private readonly Button _closeButton;
	private readonly Button _refreshButton;
	private readonly Button[] _buyButtons = new Button[Shop.OfferCount];
	private readonly Label[] _offerLabels = new Label[Shop.OfferCount];
	private readonly Control[] _offerFrames = new Control[Shop.OfferCount];
	private readonly Control.GuiInputEventHandler[] _offerGuiInputHandlers = new Control.GuiInputEventHandler[Shop.OfferCount];

	private readonly Shop _shop;
	private readonly GameManager _gameManager;
	private readonly BuildingSystem _buildingSystem;

	private bool _gameOver;
	private bool _shopButtonHovered;
	private bool _ready;
	private bool _dragging;
	private int _dragSlotIndex = -1;

	public bool IsOpen { get; private set; }
	public bool IsDragging => _dragging;
	public event Action<bool> OpenChanged;

	public bool IsPointInPanel(Vector2 screenPoint)
	{
		return _panel.GetGlobalRect().HasPoint(screenPoint);
	}

	public void SetSellHighlight(bool active)
	{
		_panel.Modulate = active ? new Color(1f, 1f, 0.75f) : Colors.White;
	}

	public ShopUiController(Node owner, CanvasLayer uiRoot, Shop shopService)
	{
		_owner = owner;
		_shop = shopService;
		_gameManager = AdapterRegistry.Resolve<GameManager>();
		_buildingSystem = AdapterRegistry.Resolve<BuildingSystem>();
		_shopButton = uiRoot.GetNode<Button>("ShopButton");
		_shopButtonCircle = uiRoot.GetNode<Panel>("ShopButtonCircle");
		_panel = uiRoot.GetNode<Panel>("ShopPanel");
		_shopGoldLabel = uiRoot.GetNode<Label>("ShopPanel/ShopGoldLabel");
		_closeButton = uiRoot.GetNode<Button>("ShopPanel/ShopCloseButton");
		ApplyShopIcons();
		_refreshButton = uiRoot.GetNode<Button>("ShopPanel/ShopRefreshButton");
		MedievalUiStyles.ApplyShop(uiRoot);
		UpdateShopButtonHighlight();

		for (int i = 0; i < Shop.OfferCount; i++)
		{
			_offerLabels[i] = uiRoot.GetNode<Label>($"ShopPanel/OfferSlot{i + 1}/OfferLabel");
			_buyButtons[i] = uiRoot.GetNode<Button>($"ShopPanel/OfferSlot{i + 1}/BuyButton");
			_offerFrames[i] = uiRoot.GetNode<Control>($"ShopPanel/OfferFrame{i + 1}");
			_offerLabels[i].MouseFilter = Control.MouseFilterEnum.Stop;

			int slotIndex = i;
			_buyButtons[i].Pressed += () => OnBuyButtonPressed(slotIndex);
			_offerGuiInputHandlers[i] = inputEvent => OnOfferGuiInput(slotIndex, inputEvent);
			_offerLabels[i].GuiInput += _offerGuiInputHandlers[i];
		}

		_shopButton.Pressed += OnShopButtonPressed;
		_shopButton.MouseEntered += OnShopButtonMouseEntered;
		_shopButton.MouseExited += OnShopButtonMouseExited;
		_closeButton.Pressed += OnClosePressed;
		_refreshButton.Pressed += OnRefreshPressed;

		_shop.GoldChanged += UpdateGoldDisplay;
		_shop.ShopAvailabilityChanged += UpdateShopButtonAvailability;
		_shop.ShopOpenRequested += Open;
		_shop.ShopOffersChanged += RefreshOffers;

		UpdateGoldDisplay(_shop.Gold);
		UpdateShopButtonAvailability(_shop.IsShopAvailable);
		RefreshOffers();
		_ready = true;
	}

	public void Dispose()
	{
		_shopButton.Pressed -= OnShopButtonPressed;
		_shopButton.MouseEntered -= OnShopButtonMouseEntered;
		_shopButton.MouseExited -= OnShopButtonMouseExited;
		_closeButton.Pressed -= OnClosePressed;
		_refreshButton.Pressed -= OnRefreshPressed;

		for (int i = 0; i < Shop.OfferCount; i++)
			_offerLabels[i].GuiInput -= _offerGuiInputHandlers[i];

		_shop.GoldChanged -= UpdateGoldDisplay;
		_shop.ShopAvailabilityChanged -= UpdateShopButtonAvailability;
		_shop.ShopOpenRequested -= Open;
		_shop.ShopOffersChanged -= RefreshOffers;
	}

	public void SetGameOver(bool gameOver)
	{
		_gameOver = gameOver;

		if (_gameOver)
			Close();

		CancelDrag();
		UpdateShopButtonAvailability(_shop.IsShopAvailable);
	}

	public void Process()
	{
		if (!_dragging)
			return;

		UpdateDragPreview();
	}

	public bool HandleInput(InputEvent @event)
	{
		if (!_dragging)
			return false;

		if (@event is InputEventMouseButton mouseButton && !mouseButton.Pressed)
		{
			if (mouseButton.ButtonIndex == MouseButton.Left)
			{
				bool placed = TryCompleteDrag(mouseButton.GlobalPosition);
				CancelDrag(!placed);
				return true;
			}

			if (mouseButton.ButtonIndex == MouseButton.Right)
			{
				CancelDrag();
				return true;
			}
		}

		return false;
	}

	public bool CancelDrag(bool playFeedback = true)
	{
		if (!_dragging)
			return false;

		_dragging = false;
		_dragSlotIndex = -1;
		_gameManager.PlayerCastle.ClearPlacementPreview();
		if (playFeedback)
			Sfx.Play(SfxEvents.PlaceCancel);
		return true;
	}

	public bool Close()
	{
		if (!IsOpen)
			return false;

		IsOpen = false;
		_shopButton.ButtonPressed = false;
		_panel.Visible = false;
		UpdateShopButtonHighlight();
		CancelDrag();
		UpdateShopButtonAvailability(_shop.IsShopAvailable);
		OpenChanged?.Invoke(IsOpen);
		Sfx.Play(SfxEvents.ShopClose);
		return true;
	}

	private void Open()
	{
		if (IsOpen || _gameOver)
			return;

		IsOpen = true;
		_shopButton.ButtonPressed = true;
		_panel.Visible = true;
		UpdateShopButtonHighlight();
		UpdateShopButtonAvailability(_shop.IsShopAvailable);
		RefreshOffers();
		OpenChanged?.Invoke(IsOpen);
		Sfx.Play(SfxEvents.ShopOpen);
	}

	private void OnShopButtonMouseEntered()
	{
		_shopButtonHovered = true;
		UpdateShopButtonHighlight();
	}

	private void OnShopButtonMouseExited()
	{
		_shopButtonHovered = false;
		UpdateShopButtonHighlight();
	}

	private void UpdateShopButtonHighlight()
	{
		MedievalUiStyles.SetShopCircleHighlight(_shopButtonCircle, _shopButtonHovered || IsOpen);
	}

	private void ApplyShopIcons()
	{
		Texture2D icon = GD.Load<Texture2D>(ShopIconPath);
		_shopButton.Icon = icon;
		_shopButton.Text = "";
	}

	private void OnShopButtonPressed()
	{
		if (_shopButton.ButtonPressed)
			_shop.RequestOpenShop();
		else
			Close();
	}

	private void OnRefreshPressed()
	{
		_shop.RefreshOffers();
	}

	private void OnClosePressed()
	{
		Close();
	}

	private void OnBuyButtonPressed(int slotIndex)
	{
		if (_shop.TryPurchase(slotIndex))
			Sfx.Play(SfxEvents.Buy);
	}

	private void OnOfferGuiInput(int slotIndex, InputEvent @event)
	{
		if (@event is not InputEventMouseButton mouseButton
			|| !mouseButton.Pressed
			|| mouseButton.ButtonIndex != MouseButton.Left)
			return;

		if (_gameOver || !_shop.IsShopAvailable)
			return;

		CardData offer = _shop.GetOffer(slotIndex);
		if (offer == null || !_shop.CanAfford(offer.Cost) || !offer.IsPlaceable)
			return;

		_dragging = true;
		_dragSlotIndex = slotIndex;
	}

	private bool TryCompleteDrag(Vector2 screenPosition)
	{
		Castle playerCastle = _gameManager.PlayerCastle;
		if (playerCastle == null || _dragSlotIndex < 0)
			return false;

		if (!playerCastle.TryGetGridFromScreenPoint(screenPosition, out int gridX, out int gridY))
			return false;

		return _shop.TryPlaceOfferDirect(_dragSlotIndex, gridX, gridY);
	}

	private void UpdateDragPreview()
	{
		Castle playerCastle = _gameManager.PlayerCastle;
		if (playerCastle == null)
			return;

		Vector2 mouseScreen = _owner.GetViewport().GetMousePosition();
		if (!playerCastle.TryGetGridFromScreenPoint(mouseScreen, out int gridX, out int gridY))
		{
			playerCastle.ClearPlacementPreview();
			return;
		}

		CardData offer = _shop.GetOffer(_dragSlotIndex);
		string buildingType = offer?.BuildingType ?? BuildingTypeIds.Barracks;
		bool valid = _buildingSystem.CanPlace(playerCastle, buildingType, gridX, gridY);
		playerCastle.SetPlacementPreview(true, gridX, gridY, valid, buildingType);
	}

	private void UpdateGoldDisplay(int gold)
	{
		_shopGoldLabel.Text = $"当前金币：{gold}";
		RefreshOffers();
		if (_ready)
			Sfx.Play(SfxEvents.CoinChange);
	}

	private void UpdateShopButtonAvailability(bool available)
	{
		_shopButton.Disabled = _gameOver || !available;
		_shopButton.Text = "";
	}

	private void RefreshOffers()
	{
		for (int i = 0; i < Shop.OfferCount; i++)
		{
			CardData offer = _shop.GetOffer(i);
			_offerFrames[i].Visible = offer != null;
			_offerLabels[i].Text = offer == null ? "" : $"{offer.Name}\n金币 {offer.Cost}";
			_buyButtons[i].Visible = offer != null;
			if (offer == null)
				continue;

			bool canBuy = _shop.CanAfford(offer.Cost);
			_buyButtons[i].Disabled = !canBuy;
			_buyButtons[i].Text = canBuy ? "购买" : "金币不足";
		}
	}
}
