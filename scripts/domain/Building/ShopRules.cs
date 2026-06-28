using System;

namespace CasualCastle.Domain.Building;

public static class ShopRules
{
    public const int OfferCount = 5;

    private static readonly CardData[] Catalog =
    {
        new() { Id = "barracks", Name = "兵营", Cost = 10, BuildingType = "Barracks" },
        new() { Id = "archery_range", Name = "靶场", Cost = 14, BuildingType = "ArcheryRange" },
        new() { Id = "stable", Name = "马厩", Cost = 18, BuildingType = "Stable" },
        new() { Id = "wolf_den", Name = "狼穴", Cost = 16, BuildingType = "WolfDen" },
    };

    public static CardData[] GetCatalog() => Catalog;

    public static CardData[] GenerateOffers(Random random)
    {
        CardData[] offers = new CardData[OfferCount];
        for (int i = 0; i < OfferCount; i++)
            offers[i] = Catalog[random.Next(Catalog.Length)];
        return offers;
    }

    public static CardData RefreshOfferSlot(Random random)
    {
        return Catalog[random.Next(Catalog.Length)];
    }
}
