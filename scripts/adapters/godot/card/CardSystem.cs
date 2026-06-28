using CasualCastle.Domain.Building;
using CasualCastle.Adapters.Godot;
using Godot;
using System.Collections.Generic;

public partial class CardSystem : Node
{
    public const int MaxHandSize = 7;

    [Signal]
    public delegate void HandChangedEventHandler();

    [Signal]
    public delegate void SelectionChangedEventHandler(int selectedIndex);

    private readonly List<CardData> _hand = new();
    private int _selectedIndex = -1;

    private BuildingSystem _buildingSystem;

    public IReadOnlyList<CardData> Hand => _hand;
    public bool HasSelection => _selectedIndex >= 0 && _selectedIndex < _hand.Count;
    public CardData SelectedCard => HasSelection ? _hand[_selectedIndex] : null;

    public override void _Ready()
    {
        AdapterRegistry.Register<CardSystem>(this);
        _buildingSystem = AdapterRegistry.Resolve<BuildingSystem>();
    }

    public override void _ExitTree()
    {
        AdapterRegistry.Unregister<CardSystem>(this);
    }

    public bool TryAddCard(CardData card)
    {
        if (card == null || _hand.Count >= MaxHandSize)
            return false;

        _hand.Add(CloneCard(card));
        EmitSignal(SignalName.HandChanged);
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
        EmitSignal(SignalName.SelectionChanged, _selectedIndex);
    }

    public void ClearSelection()
    {
        if (_selectedIndex < 0)
            return;

        _selectedIndex = -1;
        EmitSignal(SignalName.SelectionChanged, _selectedIndex);
    }

    public bool TryPlaceSelected(Castle castle, int gridX, int gridY)
    {
        if (!HasSelection)
            return false;

        return TryPlaceAtIndex(_selectedIndex, castle, gridX, gridY);
    }

    public bool TryPlaceAtIndex(int index, Castle castle, int gridX, int gridY)
    {
        if (index < 0 || index >= _hand.Count)
            return false;

        CardData card = _hand[index];
        if (!TryPlaceCard(card, castle, gridX, gridY))
            return false;

        _hand.RemoveAt(index);
        if (_selectedIndex == index)
            _selectedIndex = -1;
        else if (_selectedIndex > index)
            _selectedIndex--;

        EmitSignal(SignalName.HandChanged);
        EmitSignal(SignalName.SelectionChanged, _selectedIndex);
        return true;
    }

    public bool TryPlaceCard(CardData card, Castle castle, int gridX, int gridY)
    {
        if (card == null || castle == null || !castle.IsPlayerCastle)
            return false;

        return _buildingSystem.TryPlace(castle, card.BuildingType, gridX, gridY);
    }

    public void ResetHand()
    {
        _hand.Clear();
        _selectedIndex = -1;
        EmitSignal(SignalName.HandChanged);
        EmitSignal(SignalName.SelectionChanged, _selectedIndex);
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
