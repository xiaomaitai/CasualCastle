using System;
using System.Collections.Generic;

namespace CasualCastle.Domain.Building;

public class Hand
{
    public const int MaxHandSize = 7;

    public event Action HandChanged;
    public event Action<int> SelectionChanged;

    private readonly Player _player;
    private readonly IBuildingPlacement _placement;

    public IReadOnlyList<CardData> Cards => _player.HandCards;
    public bool HasSelection => _player.SelectedHandIndex >= 0 && _player.SelectedHandIndex < _player.HandCards.Count;
    public CardData SelectedCard => HasSelection ? _player.HandCards[_player.SelectedHandIndex] : null;

    public Hand(IBuildingPlacement placement, Player player)
    {
        _placement = placement;
        _player = player;
    }

    public bool TryAddCard(CardData card)
    {
        if (card == null || _player.HandCards.Count >= MaxHandSize)
            return false;

        _player.AddHandCard(card.Clone());
        HandChanged?.Invoke();
        return true;
    }

    public void SelectCard(int index)
    {
        if (index < 0 || index >= _player.HandCards.Count)
        {
            ClearSelection();
            return;
        }

        if (_player.SelectedHandIndex == index)
        {
            ClearSelection();
            return;
        }

        _player.SelectedHandIndex = index;
        SelectionChanged?.Invoke(_player.SelectedHandIndex);
    }

    public void ClearSelection()
    {
        if (_player.SelectedHandIndex < 0)
            return;

        _player.SelectedHandIndex = -1;
        SelectionChanged?.Invoke(_player.SelectedHandIndex);
    }

    public bool TryPlaceSelected(int gridX, int gridY)
    {
        if (!HasSelection)
            return false;

        return TryPlaceAtIndex(_player.SelectedHandIndex, gridX, gridY);
    }

    public bool TryPlaceAtIndex(int index, int gridX, int gridY)
    {
        if (index < 0 || index >= _player.HandCards.Count)
            return false;

        CardData card = _player.HandCards[index];
        if (!TryPlaceCard(card, gridX, gridY))
            return false;

        _player.RemoveHandCard(index);
        if (_player.SelectedHandIndex == index)
            _player.SelectedHandIndex = -1;
        else if (_player.SelectedHandIndex > index)
            _player.SelectedHandIndex--;

        HandChanged?.Invoke();
        SelectionChanged?.Invoke(_player.SelectedHandIndex);
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
        _player.ClearHand();
        HandChanged?.Invoke();
        SelectionChanged?.Invoke(_player.SelectedHandIndex);
    }
}
