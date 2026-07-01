using System;
using System.Collections.Generic;

namespace CasualCastle.Domain.Building;

public class Hand
{
    public const int MaxHandSize = 7;

    public event Action HandChanged;
    public event Action<int> SelectionChanged;

    private readonly List<CardData> _hand = new();
    private readonly IBuildingPlacement _placement;
    private int _selectedIndex = -1;

    public IReadOnlyList<CardData> Cards => _hand;
    public bool HasSelection => _selectedIndex >= 0 && _selectedIndex < _hand.Count;
    public CardData SelectedCard => HasSelection ? _hand[_selectedIndex] : null;

    public Hand(IBuildingPlacement placement)
    {
        _placement = placement;
    }

    public bool TryAddCard(CardData card)
    {
        if (card == null || _hand.Count >= MaxHandSize)
            return false;

        _hand.Add(CloneCard(card));
        HandChanged?.Invoke();
        return true;
    }

    public void SelectCard(int index)
    {
        if (index < 0 || index >= _hand.Count)
        {
            ClearSelection();
            return;
        }

        if (_selectedIndex == index)
        {
            ClearSelection();
            return;
        }

        _selectedIndex = index;
        SelectionChanged?.Invoke(_selectedIndex);
    }

    public void ClearSelection()
    {
        if (_selectedIndex < 0)
            return;

        _selectedIndex = -1;
        SelectionChanged?.Invoke(_selectedIndex);
    }

    public bool TryPlaceSelected(int gridX, int gridY)
    {
        if (!HasSelection)
            return false;

        return TryPlaceAtIndex(_selectedIndex, gridX, gridY);
    }

    public bool TryPlaceAtIndex(int index, int gridX, int gridY)
    {
        if (index < 0 || index >= _hand.Count)
            return false;

        CardData card = _hand[index];
        if (!TryPlaceCard(card, gridX, gridY))
            return false;

        _hand.RemoveAt(index);
        if (_selectedIndex == index)
            _selectedIndex = -1;
        else if (_selectedIndex > index)
            _selectedIndex--;

        HandChanged?.Invoke();
        SelectionChanged?.Invoke(_selectedIndex);
        return true;
    }

    public bool TryPlaceCard(CardData card, int gridX, int gridY)
    {
        if (card == null)
            return false;

        return _placement.TryPlace(card.BuildingType, gridX, gridY);
    }

    public void ResetHand()
    {
        _hand.Clear();
        _selectedIndex = -1;
        HandChanged?.Invoke();
        SelectionChanged?.Invoke(_selectedIndex);
    }

    private static CardData CloneCard(CardData source)
    {
        return new CardData
        {
            Id = source.Id,
            Name = source.Name,
            Cost = source.Cost,
            BuildingType = source.BuildingType,
        };
    }
}
