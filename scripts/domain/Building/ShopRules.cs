using System;
using System.Collections.Generic;

namespace CasualCastle.Domain.Building;

public class ShopRules
{
	public const int OfferCount = 5;

	private readonly CardData[] _allCatalog;
	private CardData[] _catalog = System.Array.Empty<CardData>();
	private int _totalWeight;

	public ShopRules(IReadOnlyList<CardData> catalog)
	{
		_allCatalog = new CardData[catalog.Count];
		for (int i = 0; i < catalog.Count; i++)
			_allCatalog[i] = catalog[i];
		SetFaction("");
	}

	public void SetFaction(string factionId)
	{
		List<CardData> filtered = new();
		foreach (CardData card in _allCatalog)
		{
			if (!string.IsNullOrEmpty(factionId) && !string.IsNullOrEmpty(card.FactionId) && card.FactionId != factionId)
				continue;
			filtered.Add(card);
		}
		_catalog = filtered.ToArray();
		_totalWeight = 0;
		foreach (CardData card in _catalog)
			_totalWeight += card.Weight;
	}

	public CardData[] GenerateOffers(Random random)
	{
		CardData[] offers = new CardData[OfferCount];
		for (int i = 0; i < OfferCount; i++)
			offers[i] = PickWeighted(random);
		return offers;
	}

	public CardData RefreshOfferSlot(Random random)
	{
		return PickWeighted(random);
	}

	private CardData PickWeighted(Random random)
	{
		int roll = random.Next(_totalWeight);
		int cumulative = 0;
		for (int i = 0; i < _catalog.Length; i++)
		{
			cumulative += _catalog[i].Weight;
			if (roll < cumulative)
				return _catalog[i];
		}
		return _catalog[_catalog.Length - 1];
	}
}
