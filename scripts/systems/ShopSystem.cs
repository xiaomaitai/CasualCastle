using Godot;
using System;
using System.Collections.Generic;

public partial class ShopSystem : Node
{
    public static ShopSystem Instance { get; private set; }

    public const int OfferCount = 3;

    [Signal]
    public delegate void GoldChangedEventHandler(int gold);

    [Signal]
    public delegate void ShopAvailabilityChangedEventHandler(bool available);

    [Signal]
    public delegate void ShopOpenRequestedEventHandler();

    [Signal]
    public delegate void ShopOffersChangedEventHandler();

    private static readonly CardData[] Catalog =
    {
        new() { Id = "barracks", Name = "兵营", Cost = 10, BuildingType = "Barracks" },
        new() { Id = "barracks_plus", Name = "强化兵营", Cost = 15, BuildingType = "Barracks" },
        new() { Id = "barracks_elite", Name = "精英兵营", Cost = 20, BuildingType = "Barracks" },
    };

    private readonly CardData[] _offers = new CardData[OfferCount];
    private readonly Random _random = new();

    public int Gold { get; private set; }
    public bool IsShopAvailable { get; private set; }

    public override void _Ready()
    {
        Instance = this;
        Gold = GameConfig.InitialGold;
        RefreshOffers();

        GameManager.Instance.PhaseChanged += OnPhaseChanged;
        GameManager.Instance.GameStateChanged += OnGameStateChanged;

        EmitSignal(SignalName.GoldChanged, Gold);
        UpdateShopAvailability();
    }

    public override void _ExitTree()
    {
        if (Instance == this)
            Instance = null;
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
        for (int i = 0; i < OfferCount; i++)
            _offers[i] = Catalog[_random.Next(Catalog.Length)];

        EmitSignal(SignalName.ShopOffersChanged);
    }

    public bool TryPurchase(int slotIndex)
    {
        if (!IsShopAvailable)
            return false;

        CardData offer = GetOffer(slotIndex);
        if (offer == null)
            return false;

        if (!CanAfford(offer.Cost))
            return false;

        if (CardSystem.Instance == null || !CardSystem.Instance.TryAddCard(offer))
            return false;

        TrySpendGold(offer.Cost);
        _offers[slotIndex] = Catalog[_random.Next(Catalog.Length)];
        EmitSignal(SignalName.ShopOffersChanged);
        return true;
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
        bool available = GameManager.Instance.CurrentState == GameManager.GameState.Playing;

        if (IsShopAvailable == available)
            return;

        IsShopAvailable = available;
        EmitSignal(SignalName.ShopAvailabilityChanged, IsShopAvailable);
    }
}
