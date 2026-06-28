using CasualCastle.Domain.Building;
using CasualCastle.Adapters.Godot;
using Godot;
using System;

public partial class ShopSystem : Node
{
    public static ShopSystem Instance { get; private set; }

    public const int OfferCount = 5;

    [Signal]
    public delegate void GoldChangedEventHandler(int gold);

    [Signal]
    public delegate void ShopAvailabilityChangedEventHandler(bool available);

    [Signal]
    public delegate void ShopOpenRequestedEventHandler();

    [Signal]
    public delegate void ShopOffersChangedEventHandler();

    private readonly CardData[] _offers = new CardData[OfferCount];
    private readonly Random _random = new();

    private CardSystem _cardSystem;
    private GameManager _gameManager;

    public int Gold { get; private set; }
    public bool IsShopAvailable { get; private set; }

    public override void _Ready()
    {
        Instance = this;
        AdapterRegistry.Register<ShopSystem>(this);
        _cardSystem = AdapterRegistry.Resolve<CardSystem>();
        _gameManager = AdapterRegistry.Resolve<GameManager>();

        Gold = GameConfig.InitialGold;
        RefreshOffers();

        _gameManager.PhaseChanged += OnPhaseChanged;
        _gameManager.GameStateChanged += OnGameStateChanged;

        EmitSignal(SignalName.GoldChanged, Gold);
        UpdateShopAvailability();
    }

    public override void _ExitTree()
    {
        _gameManager.PhaseChanged -= OnPhaseChanged;
        _gameManager.GameStateChanged -= OnGameStateChanged;

        if (Instance == this)
        {
            AdapterRegistry.Unregister<ShopSystem>(this);
            Instance = null;
        }
    }

    public CardData GetOffer(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= OfferCount)
            return null;
        return _offers[slotIndex];
    }

    public bool CanAfford(int cost) => Gold >= cost;

    public bool TrySpendGold(int cost)
    {
        if (Gold < cost)
            return false;
        Gold -= cost;
        EmitSignal(SignalName.GoldChanged, Gold);
        return true;
    }

    public void AddGold(int amount)
    {
        Gold += amount;
        EmitSignal(SignalName.GoldChanged, Gold);
    }

    public void RefreshOffers()
    {
        CardData[] generated = ShopRules.GenerateOffers(_random);
        for (int i = 0; i < OfferCount; i++)
            _offers[i] = generated[i];
        EmitSignal(SignalName.ShopOffersChanged);
    }

    public bool TryPurchase(int slotIndex)
    {
        if (!IsShopAvailable)
            return false;

        CardData offer = GetOffer(slotIndex);
        if (offer == null || !CanAfford(offer.Cost))
            return false;

        if (!_cardSystem.TryAddCard(offer))
            return false;

        TrySpendGold(offer.Cost);
        RefreshOfferSlot(slotIndex);
        return true;
    }

    public bool TryPlaceOfferDirect(int slotIndex, Castle castle, int gridX, int gridY)
    {
        if (!IsShopAvailable)
            return false;

        CardData offer = GetOffer(slotIndex);
        if (offer == null || !CanAfford(offer.Cost))
            return false;

        if (!_cardSystem.TryPlaceCard(offer, castle, gridX, gridY))
            return false;

        TrySpendGold(offer.Cost);
        RefreshOfferSlot(slotIndex);
        return true;
    }

    public bool IsRepairAvailable =>
        _gameManager.CurrentState == GameManager.GameState.Playing
        && _gameManager.IsNight;

    public bool TryRepairBuilding(Building building)
    {
        return building.TryRepair();
    }

    private void RefreshOfferSlot(int slotIndex)
    {
        _offers[slotIndex] = ShopRules.RefreshOfferSlot(_random);
        EmitSignal(SignalName.ShopOffersChanged);
    }

    public void RequestOpenShop()
    {
        if (!IsShopAvailable)
            return;
        EmitSignal(SignalName.ShopOpenRequested);
    }

    private void OnPhaseChanged(GameManager.GamePhase phase)
    {
        UpdateShopAvailability();
        if (phase == GameManager.GamePhase.Night)
            RequestOpenShop();
    }

    private void OnGameStateChanged(GameManager.GameState state)
    {
        UpdateShopAvailability();
    }

    private void UpdateShopAvailability()
    {
        bool available = _gameManager.CurrentState == GameManager.GameState.Playing;
        if (IsShopAvailable == available)
            return;
        IsShopAvailable = available;
        EmitSignal(SignalName.ShopAvailabilityChanged, IsShopAvailable);
    }
}
