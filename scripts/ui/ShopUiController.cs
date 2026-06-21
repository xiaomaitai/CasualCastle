using Godot;
using System;

public sealed class ShopUiController
{
    private readonly Button _shopButton;
    private readonly ColorRect _overlay;
    private readonly Panel _panel;
    private readonly Label _shopGoldLabel;
    private readonly Button _closeButton;
    private readonly Button _refreshButton;
    private readonly Button[] _buyButtons = new Button[ShopSystem.OfferCount];
    private readonly Label[] _offerLabels = new Label[ShopSystem.OfferCount];

    private bool _gameOver;

    public bool IsOpen { get; private set; }
    public event Action<bool> OpenChanged;

    public ShopUiController(CanvasLayer uiRoot)
    {
        _shopButton = uiRoot.GetNode<Button>("ShopButton");
        _overlay = uiRoot.GetNode<ColorRect>("ShopOverlay");
        _panel = uiRoot.GetNode<Panel>("ShopPanel");
        _shopGoldLabel = uiRoot.GetNode<Label>("ShopPanel/ShopGoldLabel");
        _closeButton = uiRoot.GetNode<Button>("ShopPanel/ShopCloseButton");
        _refreshButton = uiRoot.GetNode<Button>("ShopPanel/ShopRefreshButton");

        for (int i = 0; i < ShopSystem.OfferCount; i++)
        {
            _offerLabels[i] = uiRoot.GetNode<Label>($"ShopPanel/OfferSlot{i + 1}/OfferLabel");
            _buyButtons[i] = uiRoot.GetNode<Button>($"ShopPanel/OfferSlot{i + 1}/BuyButton");
            int slotIndex = i;
            _buyButtons[i].Pressed += () => OnBuyButtonPressed(slotIndex);
        }

        _shopButton.Pressed += OnShopButtonPressed;
        _closeButton.Pressed += OnClosePressed;
        _refreshButton.Pressed += OnRefreshPressed;

        if (ShopSystem.Instance != null)
        {
            ShopSystem.Instance.GoldChanged += UpdateGoldDisplay;
            ShopSystem.Instance.ShopAvailabilityChanged += UpdateShopButtonAvailability;
            ShopSystem.Instance.ShopOpenRequested += Open;
            ShopSystem.Instance.ShopOffersChanged += RefreshOffers;

            UpdateGoldDisplay(ShopSystem.Instance.Gold);
            UpdateShopButtonAvailability(ShopSystem.Instance.IsShopAvailable);
            RefreshOffers();
        }
    }

    public void Dispose()
    {
        _shopButton.Pressed -= OnShopButtonPressed;
        _closeButton.Pressed -= OnClosePressed;
        _refreshButton.Pressed -= OnRefreshPressed;

        if (ShopSystem.Instance != null)
        {
            ShopSystem.Instance.GoldChanged -= UpdateGoldDisplay;
            ShopSystem.Instance.ShopAvailabilityChanged -= UpdateShopButtonAvailability;
            ShopSystem.Instance.ShopOpenRequested -= Open;
            ShopSystem.Instance.ShopOffersChanged -= RefreshOffers;
        }
    }

    public void SetGameOver(bool gameOver)
    {
        _gameOver = gameOver;

        if (_gameOver)
            Close();

        UpdateShopButtonAvailability(ShopSystem.Instance?.IsShopAvailable == true);
    }

    public bool Close()
    {
        if (!IsOpen)
            return false;

        IsOpen = false;
        _overlay.Visible = false;
        _panel.Visible = false;
        UpdateShopButtonAvailability(ShopSystem.Instance?.IsShopAvailable == true);
        OpenChanged?.Invoke(IsOpen);
        return true;
    }

    private void Open()
    {
        if (IsOpen || _gameOver || ShopSystem.Instance?.IsShopAvailable != true)
            return;

        IsOpen = true;
        CardSystem.Instance?.ClearSelection();
        _overlay.Visible = true;
        _panel.Visible = true;
        UpdateShopButtonAvailability(ShopSystem.Instance.IsShopAvailable);
        RefreshOffers();
        OpenChanged?.Invoke(IsOpen);
    }

    private void OnShopButtonPressed()
    {
        ShopSystem.Instance?.RequestOpenShop();
    }

    private void OnRefreshPressed()
    {
        ShopSystem.Instance?.RefreshOffers();
    }

    private void OnClosePressed()
    {
        Close();
    }

    private void OnBuyButtonPressed(int slotIndex)
    {
        ShopSystem.Instance?.TryPurchase(slotIndex);
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
        if (ShopSystem.Instance == null)
            return;

        for (int i = 0; i < ShopSystem.OfferCount; i++)
        {
            CardData offer = ShopSystem.Instance.GetOffer(i);
            _offerLabels[i].Text = offer == null
                ? "暂无商品"
                : $"{offer.Name}  费用：{offer.Cost}";

            bool canBuy = offer != null && ShopSystem.Instance.CanAfford(offer.Cost);
            _buyButtons[i].Disabled = offer == null || !canBuy;
            _buyButtons[i].Text = canBuy ? "购买" : "金币不足";
        }
    }
}
