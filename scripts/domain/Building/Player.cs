using System.Collections.Generic;

namespace CasualCastle.Domain.Building;

public class Player
{
    private readonly List<CardData> _handCards = new();
    private readonly CardData[] _shopOffers = new CardData[ShopRules.OfferCount];

    public int Gold { get; set; }
    public IReadOnlyList<CardData> HandCards => _handCards;
    public CardData[] ShopOffers => _shopOffers;
    public int SelectedHandIndex { get; set; } = -1;
    public bool IsShopAvailable { get; set; }

    public void AddHandCard(CardData card)
    {
        _handCards.Add(card);
    }

    public void RemoveHandCard(int index)
    {
        _handCards.RemoveAt(index);
    }

    public void ClearHand()
    {
        _handCards.Clear();
        SelectedHandIndex = -1;
    }
}
