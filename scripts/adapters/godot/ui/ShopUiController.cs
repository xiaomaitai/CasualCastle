// 商店组件：购买与直接放置（shop_panel.tscn 场景根）
using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Battle;
using CasualCastle.Domain.Building;
using CasualCastle.Domain.Shared;
using Godot;
using System;

public partial class ShopUiController : Control
{
	private const string ShopIconPath = "res://assets/art/ui/shop_goods_pile_194.png";
	private static readonly Color GoldInsufficientColor = new(0.9f, 0.25f, 0.2f, 1f);

	private Button _shopButton;
	private Panel _shopButtonCircle;
	private Panel _panel;
	private Label _shopGoldLabel;
	private Button _closeButton;
	private Button _refreshButton;
	private readonly Button[] _buyButtons = new Button[Shop.OfferCount];
	private readonly Label[] _offerLabels = new Label[Shop.OfferCount];
	private readonly OfferFrame[] _offerFrames = new OfferFrame[Shop.OfferCount];
	private readonly Control.GuiInputEventHandler[] _offerGuiInputHandlers = new Control.GuiInputEventHandler[Shop.OfferCount];

	private Shop _shop;
	private GameManager _gameManager;
	private BuildingSystem _buildingSystem;

	private bool _gameOver;
	private bool _shopButtonHovered;
	private bool _ready;
	private bool _dragging;
	private int _dragSlotIndex = -1;
	private UiProfileKind _profile;

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

	public override void _Ready()
	{
		_shop = AdapterRegistry.Resolve<Shop>();
		_gameManager = AdapterRegistry.Resolve<GameManager>();
		_buildingSystem = AdapterRegistry.Resolve<BuildingSystem>();
		_shopButton = GetNode<Button>("ShopButton");
		_shopButtonCircle = GetNode<Panel>("ShopButtonCircle");
		_panel = GetNode<Panel>("ShopPanel");
		_shopGoldLabel = GetNode<Label>("ShopPanel/ShopGoldLabel");
		_closeButton = GetNode<Button>("ShopPanel/ShopCloseButton");
		_refreshButton = GetNode<Button>("ShopPanel/ShopRefreshButton");
		AdapterRegistry.Register<ShopUiController>(this);
		ApplyShopIcons();
		DisplaySettingsManager settings = AdapterRegistry.Resolve<DisplaySettingsManager>();
		_profile = settings.UiProfile;
		UiProfileStyles.ApplyShop(this, _profile);
		settings.UiProfileChanged += ApplyProfile;
		UpdateShopButtonHighlight();

		for (int i = 0; i < Shop.OfferCount; i++)
		{
			_offerLabels[i] = GetNode<Label>($"ShopPanel/OfferSlot{i + 1}/OfferLabel");
			_buyButtons[i] = GetNode<Button>($"ShopPanel/OfferSlot{i + 1}/BuyButton");
			_offerFrames[i] = GetNode<OfferFrame>($"ShopPanel/OfferFrame{i + 1}");
			_offerLabels[i].MouseFilter = Control.MouseFilterEnum.Stop;

			int slotIndex = i;
			_buyButtons[i].Pressed += () => OnBuyButtonPressed(slotIndex);
			_offerGuiInputHandlers[i] = inputEvent => OnOfferGuiInput(slotIndex, inputEvent);
			_offerLabels[i].GuiInput += _offerGuiInputHandlers[i];
		}

		ApplyOfferFrames();

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

	public override void _ExitTree()
	{
		AdapterRegistry.Resolve<DisplaySettingsManager>().UiProfileChanged -= ApplyProfile;
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
		AdapterRegistry.Unregister<ShopUiController>(this);
	}

	private void ApplyProfile(UiProfileKind kind)
	{
		_profile = kind;
		UiProfileStyles.ApplyShop(this, kind);
		ApplyOfferFrames();
		UpdateShopButtonHighlight();
	}

	private void ApplyOfferFrames()
	{
		for (int i = 0; i < _offerFrames.Length; i++)
			_offerFrames[i].ApplyProfile(_profile);
	}

	public void SetGameOver(bool gameOver)
	{
		_gameOver = gameOver;

		if (_gameOver)
			Close();

		CancelDrag();
		UpdateShopButtonAvailability(_shop.IsShopAvailable);
	}

	public override void _Process(double _)
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

	public bool ToggleOpen()
	{
		if (IsOpen)
		{
			Close();
			return true;
		}
		if (!_shop.IsShopAvailable || _gameOver)
			return false;
		Open();
		return true;
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
		UiProfileStyles.SetShopCircleHighlight(_shopButtonCircle, _shopButtonHovered || IsOpen, _profile);
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
		else
			PlayGoldInsufficientFlash();
	}

	public bool TryBuySlot(int slotIndex)
	{
		if (slotIndex < 0 || slotIndex >= Shop.OfferCount || !IsOpen || _gameOver)
			return false;
		OnBuyButtonPressed(slotIndex);
		return true;
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

		Vector2 mouseScreen = GetViewport().GetMousePosition();
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
		PlayGoldPop();
		RefreshOffers();
		if (_ready)
			Sfx.Play(SfxEvents.CoinChange);
	}

	private void PlayGoldPop()
	{
		Tween tween = CreateTween();
		_shopGoldLabel.PivotOffset = _shopGoldLabel.Size / 2f;
		tween.TweenProperty(_shopGoldLabel, "scale", new Vector2(1.12f, 1.12f), 0.08f);
		tween.TweenProperty(_shopGoldLabel, "scale", Vector2.One, 0.12f);
	}

	private void PlayGoldInsufficientFlash()
	{
		Tween tween = CreateTween();
		_shopGoldLabel.AddThemeColorOverride("font_color", GoldInsufficientColor);
		tween.TweenInterval(0.5f);
		tween.TweenCallback(Callable.From(() => _shopGoldLabel.RemoveThemeColorOverride("font_color")));
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

			_offerLabels[i].TooltipText = BuildOfferTooltip(offer);
			bool canBuy = _shop.CanAfford(offer.Cost);
			_buyButtons[i].Disabled = !canBuy;
			_buyButtons[i].Text = canBuy ? "购买" : "金币不足";
		}
	}

	private string BuildOfferTooltip(CardData offer)
	{
		if (CardTooltipBuilder.IsItemCard(offer))
			return CardTooltipBuilder.BuildItemTooltip(offer);
		string unitTypeId = GameManager.Get<IBuildingRepository>().GetUnitTypeId(offer.BuildingType);
		if (string.IsNullOrEmpty(unitTypeId))
			return $"{offer.Name}：不产出单位";
		UnitStats stats = GameManager.Get<IUnitRepository>().Get(unitTypeId);
		string attack = stats.HealAmount > 0f
			? $"治疗 {stats.HealAmount:0}"
			: $"攻击 {stats.Damage}";
		string unitName = UnitDisplayName.Get(unitTypeId);
		string unitLine = $"产出：{unitName}  生命 {stats.Health}  {attack}";
		string rangeLine = stats.HealAmount > 0f
			? $"治疗范围 {stats.HealRange:0}  间隔 {stats.HealInterval:0.##}s"
			: $"射程 {stats.AttackRange:0}  间隔 {stats.AttackCooldown:0.##}s";
		return $"{unitLine}\n{rangeLine}\n移速 {stats.Speed:0}  生产 {_buildingSystem.GetSpawnInterval(offer.BuildingType):0.##}s";
	}
}