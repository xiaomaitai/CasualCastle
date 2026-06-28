using System;
using System.Collections.Generic;

namespace CasualCastle.Domain.Building;

public static class ShopRules
{
    public const int OfferCount = 5;

    private static CardData[] _catalog = Array.Empty<CardData>();

    public static void LoadCatalog(List<CardData> catalog)
    {
        _catalog = catalog.ToArray();
    }

    public static CardData[] GetCatalog() => _catalog;

    public static CardData[] GenerateOffers(Random random)
    {
        CardData[] offers = new CardData[OfferCount];
        for (int i = 0; i < OfferCount; i++)
            offers[i] = _catalog[random.Next(_catalog.Length)];
        return offers;
    }

    public static CardData RefreshOfferSlot(Random random)
    {
        return _catalog[random.Next(_catalog.Length)];
    }
}
