using CasualCastle.Domain.Shared;
using System;

namespace CasualCastle.Domain.Building;

public class ShopService
{
    public const int OfferCount = 5;

    public event Action<int> GoldChanged;
    public event Action<bool> ShopAvailabilityChanged;
    public event Action ShopOpenRequested;
    public event Action ShopOffersChanged;

    private readonly CardData[] _offers = new CardData[OfferCount];
    private readonly Random _random = new();
    private readonly HandService _handService;

    public int Gold { get; private set; }
    public bool IsShopAvailable { get; private set; }

    public ShopService(HandService handService)
    {
        _handService = handService;
        Gold = GameRules.InitialGold;
        RefreshOffers();
        GoldChanged?.Invoke(Gold);
        SetShopAvailable(true);
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
        GoldChanged?.Invoke(Gold);
        return true;
    }

    public void AddGold(int amount)
    {
        Gold += amount;
        GoldChanged?.Invoke(Gold);
    }

    public void RefreshOffers()
    {
        CardData[] generated = ShopRules.GenerateOffers(_random);
        for (int i = 0; i < OfferCount; i++)
            _offers[i] = generated[i];
        ShopOffersChanged?.Invoke();
    }

    public bool TryPurchase(int slotIndex)
    {
        if (!IsShopAvailable)
            return false;

        CardData offer = GetOffer(slotIndex);
        if (offer == null || !CanAfford(offer.Cost))
            return false;

        if (!_handService.TryAddCard(offer))
            return false;

        TrySpendGold(offer.Cost);
        RefreshOfferSlot(slotIndex);
        return true;
    }

    public bool TryPlaceOfferDirect(int slotIndex, int gridX, int gridY)
    {
        if (!IsShopAvailable)
            return false;

        CardData offer = GetOffer(slotIndex);
        if (offer == null || !CanAfford(offer.Cost))
            return false;

        if (!_handService.TryPlaceCard(offer, gridX, gridY))
            return false;

        TrySpendGold(offer.Cost);
        RefreshOfferSlot(slotIndex);
        return true;
    }

    public void SetShopAvailable(bool available)
    {
        if (IsShopAvailable == available)
            return;
        IsShopAvailable = available;
        ShopAvailabilityChanged?.Invoke(IsShopAvailable);
    }

    public void RequestOpenShop()
    {
        if (!IsShopAvailable)
            return;
        ShopOpenRequested?.Invoke();
    }

    public bool IsRepairAvailable(bool isPlaying, bool isNight)
    {
        return isPlaying && isNight;
    }

    private void RefreshOfferSlot(int slotIndex)
    {
        _offers[slotIndex] = ShopRules.RefreshOfferSlot(_random);
        ShopOffersChanged?.Invoke();
    }
}
