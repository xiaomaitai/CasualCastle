using Godot;
using System.Collections.Generic;

public partial class CardSystem : Node
{
    public static CardSystem Instance { get; private set; }

    public const int MaxHandSize = 5;

    [Signal]
    public delegate void HandChangedEventHandler();

    [Signal]
    public delegate void SelectionChangedEventHandler(int selectedIndex);

    private readonly List<CardData> _hand = new();
    private int _selectedIndex = -1;

    public IReadOnlyList<CardData> Hand => _hand;
    public bool HasSelection => _selectedIndex >= 0 && _selectedIndex < _hand.Count;
    public CardData SelectedCard => HasSelection ? _hand[_selectedIndex] : null;

    public override void _Ready()
    {
        Instance = this;
    }

    public override void _ExitTree()
    {
        if (Instance == this)
            Instance = null;
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
        if (!HasSelection || castle == null || !castle.IsPlayerCastle)
            return false;

        CardData card = SelectedCard;
        if (card.BuildingType != "Barracks")
            return false;

        if (!castle.IsCellPassable(gridX, gridY))
            return false;

        PackedScene scene = GD.Load<PackedScene>("res://prefabs/Barracks.tscn");
        if (scene == null)
            return false;

        Barracks barracks = scene.Instantiate<Barracks>();
        barracks.BindToGrid(castle, gridX, gridY);
        if (!castle.PlaceBuilding(barracks, gridX, gridY))
        {
            barracks.QueueFree();
            return false;
        }

        _hand.RemoveAt(_selectedIndex);
        _selectedIndex = -1;
        EmitSignal(SignalName.HandChanged);
        EmitSignal(SignalName.SelectionChanged, _selectedIndex);
        return true;
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
