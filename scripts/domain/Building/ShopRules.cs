using System;
using System.Collections.Generic;

namespace CasualCastle.Domain.Building;

public class ShopRules
{
	public const int OfferCount = 5;

	private readonly CardData[] _catalog;
	private readonly int _totalWeight;

	public ShopRules(IReadOnlyList<CardData> catalog)
	{
		_catalog = new CardData[catalog.Count];
		for (int i = 0; i < catalog.Count; i++)
		{
			_catalog[i] = catalog[i];
			_totalWeight += catalog[i].Weight;
		}
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
