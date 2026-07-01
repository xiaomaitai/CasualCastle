using System;
using System.Collections.Generic;

namespace CasualCastle.Domain.Building;

public class ShopRules
{
	public const int OfferCount = 5;

	private readonly CardData[] _catalog;

	public ShopRules(IReadOnlyList<CardData> catalog)
	{
		_catalog = new CardData[catalog.Count];
		for (int i = 0; i < catalog.Count; i++)
			_catalog[i] = catalog[i];
	}

	public CardData[] GenerateOffers(Random random)
	{
		CardData[] offers = new CardData[OfferCount];
		for (int i = 0; i < OfferCount; i++)
			offers[i] = _catalog[random.Next(_catalog.Length)];
		return offers;
	}

	public CardData RefreshOfferSlot(Random random)
	{
		return _catalog[random.Next(_catalog.Length)];
	}
}
