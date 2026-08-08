// 商店领域服务：购买、刷新与金币
using CasualCastle.Domain.Shared;
using System;

namespace CasualCastle.Domain.Building;

public class Shop
{
	public const int OfferCount = ShopRules.OfferCount;

	public event Action<int> GoldChanged;
	public event Action<bool> ShopAvailabilityChanged;
	public event Action ShopOpenRequested;
	public event Action ShopOffersChanged;

	private readonly Player _player;
	private readonly Random _random = new();
	private readonly Hand _hand;
	private readonly ShopRules _shopRules;

	public int Gold => _player.Gold;
	public bool IsShopAvailable => _player.IsShopAvailable;
	public string FactionId => _player.FactionId;

	public Shop(Hand handService, ShopRules shopRules, Player player)
	{
		_hand = handService;
		_shopRules = shopRules;
		_player = player;
		_player.Gold = GameRules.InitialGold;
		RefreshOffers();
		GoldChanged?.Invoke(_player.Gold);
		SetShopAvailable(true);
	}

	public CardData GetOffer(int slotIndex)
	{
		if (slotIndex < 0 || slotIndex >= OfferCount)
			return null;
		return _player.ShopOffers[slotIndex];
	}

	public bool CanAfford(int cost) => _player.Gold >= cost;

	public bool TrySpendGold(int cost)
	{
		if (_player.Gold < cost)
			return false;
		_player.Gold -= cost;
		GoldChanged?.Invoke(_player.Gold);
		return true;
	}

	public void AddGold(int amount)
	{
		_player.Gold += amount;
		GoldChanged?.Invoke(_player.Gold);
	}

	public void SetFaction(string factionId)
	{
		if (_player.FactionId == factionId)
			return;

		_player.FactionId = factionId;
		_shopRules.SetFaction(factionId);
		RefreshOffers();
	}

	public void RefreshOffers()
	{
		CardData[] generated = _shopRules.GenerateOffers(_random);
		for (int i = 0; i < OfferCount; i++)
			_player.ShopOffers[i] = generated[i];
		ShopOffersChanged?.Invoke();
	}

	public bool TryPurchase(int slotIndex)
	{
		if (!_player.IsShopAvailable)
			return false;

		CardData offer = GetOffer(slotIndex);
		if (offer == null || !CanAfford(offer.Cost))
			return false;

		if (!_hand.TryAddCard(offer))
			return false;

		TrySpendGold(offer.Cost);
		RefreshOfferSlot(slotIndex);
		return true;
	}

	public bool TrySellHandCard(int handIndex)
	{
		if (!_hand.TryRemoveAtIndex(handIndex, out CardData card))
			return false;

		AddGold(SellRules.GetSellPrice(card.Cost));
		return true;
	}

	public bool TryPlaceOfferDirect(int slotIndex, int gridX, int gridY)
	{
		if (!_player.IsShopAvailable)
			return false;

		CardData offer = GetOffer(slotIndex);
		if (offer == null || !CanAfford(offer.Cost))
			return false;

		if (!_hand.TryPlaceCard(offer, gridX, gridY))
			return false;

		TrySpendGold(offer.Cost);
		RefreshOfferSlot(slotIndex);
		return true;
	}

	public void SetShopAvailable(bool available)
	{
		if (_player.IsShopAvailable == available)
			return;
		_player.IsShopAvailable = available;
		ShopAvailabilityChanged?.Invoke(_player.IsShopAvailable);
	}

	public void RequestOpenShop()
	{
		if (!_player.IsShopAvailable)
			return;
		ShopOpenRequested?.Invoke();
	}


	private void RefreshOfferSlot(int slotIndex)
	{
		_player.ShopOffers[slotIndex] = _shopRules.RefreshOfferSlot(_random);
		ShopOffersChanged?.Invoke();
	}
}
