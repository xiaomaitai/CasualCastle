using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Building;
using Godot;
using System;

public sealed class ShopUiController
{
    private readonly Node _owner;
    private readonly Button _shopButton;
    private readonly Panel _panel;
    private readonly Label _shopGoldLabel;
    private readonly Button _closeButton;
    private readonly Button _refreshButton;
    private readonly Button[] _buyButtons = new Button[ShopSystem.OfferCount];
    private readonly Label[] _offerLabels = new Label[ShopSystem.OfferCount];
    private readonly Control.GuiInputEventHandler[] _offerGuiInputHandlers = new Control.GuiInputEventHandler[ShopSystem.OfferCount];

    private bool _gameOver;
    private bool _dragging;
    private int _dragSlotIndex = -1;

    public bool IsOpen { get; private set; }
    public bool IsDragging => _dragging;
    public event Action<bool> OpenChanged;

    public ShopUiController(Node owner, CanvasLayer uiRoot)
    {
        _owner = owner;
        _shopButton = uiRoot.GetNode<Button>("ShopButton");
        _panel = uiRoot.GetNode<Panel>("ShopPanel");
        _shopGoldLabel = uiRoot.GetNode<Label>("ShopPanel/ShopGoldLabel");
        _closeButton = uiRoot.GetNode<Button>("ShopPanel/ShopCloseButton");
        _refreshButton = uiRoot.GetNode<Button>("ShopPanel/ShopRefreshButton");

        for (int i = 0; i < ShopSystem.OfferCount; i++)
        {
            _offerLabels[i] = uiRoot.GetNode<Label>($"ShopPanel/OfferSlot{i + 1}/OfferLabel");
            _buyButtons[i] = uiRoot.GetNode<Button>($"ShopPanel/OfferSlot{i + 1}/BuyButton");
            _offerLabels[i].MouseFilter = Control.MouseFilterEnum.Stop;

            int slotIndex = i;
            _buyButtons[i].Pressed += () => OnBuyButtonPressed(slotIndex);
            _offerGuiInputHandlers[i] = inputEvent => OnOfferGuiInput(slotIndex, inputEvent);
            _offerLabels[i].GuiInput += _offerGuiInputHandlers[i];
        }

        _shopButton.Pressed += OnShopButtonPressed;
        _closeButton.Pressed += OnClosePressed;
        _refreshButton.Pressed += OnRefreshPressed;

        if (AdapterRegistry.Resolve<ShopSystem>() != null)
        {
            AdapterRegistry.Resolve<ShopSystem>().GoldChanged += UpdateGoldDisplay;
            AdapterRegistry.Resolve<ShopSystem>().ShopAvailabilityChanged += UpdateShopButtonAvailability;
            AdapterRegistry.Resolve<ShopSystem>().ShopOpenRequested += Open;
            AdapterRegistry.Resolve<ShopSystem>().ShopOffersChanged += RefreshOffers;

            UpdateGoldDisplay(AdapterRegistry.Resolve<ShopSystem>().Gold);
            UpdateShopButtonAvailability(AdapterRegistry.Resolve<ShopSystem>().IsShopAvailable);
            RefreshOffers();
        }
    }

    public void Dispose()
    {
        _shopButton.Pressed -= OnShopButtonPressed;
        _closeButton.Pressed -= OnClosePressed;
        _refreshButton.Pressed -= OnRefreshPressed;

        for (int i = 0; i < ShopSystem.OfferCount; i++)
            _offerLabels[i].GuiInput -= _offerGuiInputHandlers[i];

        if (AdapterRegistry.Resolve<ShopSystem>() != null)
        {
            AdapterRegistry.Resolve<ShopSystem>().GoldChanged -= UpdateGoldDisplay;
            AdapterRegistry.Resolve<ShopSystem>().ShopAvailabilityChanged -= UpdateShopButtonAvailability;
            AdapterRegistry.Resolve<ShopSystem>().ShopOpenRequested -= Open;
            AdapterRegistry.Resolve<ShopSystem>().ShopOffersChanged -= RefreshOffers;
        }
    }

    public void SetGameOver(bool gameOver)
    {
        _gameOver = gameOver;

        if (_gameOver)
            Close();

        CancelDrag();
        UpdateShopButtonAvailability(AdapterRegistry.Resolve<ShopSystem>()?.IsShopAvailable == true);
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
                TryCompleteDrag(mouseButton.GlobalPosition);
                CancelDrag();
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

    public bool CancelDrag()
    {
        if (!_dragging)
            return false;

        _dragging = false;
        _dragSlotIndex = -1;
        AdapterRegistry.Resolve<GameManager>().PlayerCastle?.ClearPlacementPreview();
        return true;
    }

    public bool Close()
    {
        if (!IsOpen)
            return false;

        IsOpen = false;
        _panel.Visible = false;
        CancelDrag();
        UpdateShopButtonAvailability(AdapterRegistry.Resolve<ShopSystem>()?.IsShopAvailable == true);
        OpenChanged?.Invoke(IsOpen);
        return true;
    }

    private void Open()
    {
        if (IsOpen || _gameOver || AdapterRegistry.Resolve<ShopSystem>()?.IsShopAvailable != true)
            return;

        IsOpen = true;
        _panel.Visible = true;
        UpdateShopButtonAvailability(AdapterRegistry.Resolve<ShopSystem>().IsShopAvailable);
        RefreshOffers();
        OpenChanged?.Invoke(IsOpen);
    }

    private void OnShopButtonPressed()
    {
        AdapterRegistry.Resolve<ShopSystem>()?.RequestOpenShop();
    }

    private void OnRefreshPressed()
    {
        AdapterRegistry.Resolve<ShopSystem>()?.RefreshOffers();
    }

    private void OnClosePressed()
    {
        Close();
    }

    private void OnBuyButtonPressed(int slotIndex)
    {
        AdapterRegistry.Resolve<ShopSystem>()?.TryPurchase(slotIndex);
    }

    private void OnOfferGuiInput(int slotIndex, InputEvent @event)
    {
        if (@event is not InputEventMouseButton mouseButton
            || !mouseButton.Pressed
            || mouseButton.ButtonIndex != MouseButton.Left)
            return;

        if (_gameOver || AdapterRegistry.Resolve<ShopSystem>()?.IsShopAvailable != true)
            return;

        CardData offer = AdapterRegistry.Resolve<ShopSystem>().GetOffer(slotIndex);
        if (offer == null || !AdapterRegistry.Resolve<ShopSystem>().CanAfford(offer.Cost))
            return;

        _dragging = true;
        _dragSlotIndex = slotIndex;
    }

    private void TryCompleteDrag(Vector2 globalPosition)
    {
        Castle playerCastle = AdapterRegistry.Resolve<GameManager>().PlayerCastle;
        if (playerCastle == null || AdapterRegistry.Resolve<ShopSystem>() == null || _dragSlotIndex < 0)
            return;

        if (!playerCastle.TryGetGridFromGlobalPoint(globalPosition, out int gridX, out int gridY))
            return;

        AdapterRegistry.Resolve<ShopSystem>().TryPlaceOfferDirect(_dragSlotIndex, playerCastle, gridX, gridY);
    }

    private void UpdateDragPreview()
    {
        Castle playerCastle = AdapterRegistry.Resolve<GameManager>().PlayerCastle;
        if (playerCastle == null)
            return;

        Vector2 mouseGlobal = _owner.GetViewport().GetMousePosition();
        if (!playerCastle.TryGetGridFromGlobalPoint(mouseGlobal, out int gridX, out int gridY))
        {
            playerCastle.ClearPlacementPreview();
            return;
        }

        CardData offer = AdapterRegistry.Resolve<ShopSystem>().GetOffer(_dragSlotIndex);
        string buildingType = offer?.BuildingType ?? "Barracks";
        bool valid = AdapterRegistry.Resolve<BuildingSystem>()?.CanPlace(playerCastle, buildingType, gridX, gridY) == true;
        playerCastle.SetPlacementPreview(true, gridX, gridY, valid, buildingType);
    }

    private void UpdateGoldDisplay(int gold)
    {
        _shopGoldLabel.Text = $"当前金币：{gold}";
        RefreshOffers();
    }

    private void UpdateShopButtonAvailability(bool available)
    {
        _shopButton.Disabled = _gameOver || !available || IsOpen;
        _shopButton.Text = "商店";
    }

    private void RefreshOffers()
    {
        if (AdapterRegistry.Resolve<ShopSystem>() == null)
            return;

        for (int i = 0; i < ShopSystem.OfferCount; i++)
        {
            CardData offer = AdapterRegistry.Resolve<ShopSystem>().GetOffer(i);
            _offerLabels[i].Text = offer == null
                ? "暂无商品"
                : $"{offer.Name}  费用：{offer.Cost}";

            bool canBuy = offer != null && AdapterRegistry.Resolve<ShopSystem>().CanAfford(offer.Cost);
            _buyButtons[i].Disabled = offer == null || !canBuy;
            _buyButtons[i].Text = canBuy ? "购买" : "金币不足";
        }
    }
}
