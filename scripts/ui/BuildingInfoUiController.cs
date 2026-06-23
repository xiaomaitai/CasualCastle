using Godot;
using System.Collections.Generic;

public sealed class BuildingInfoUiController
{
    private readonly Node _owner;
    private readonly Panel _panel;
    private readonly Label _nameLabel;
    private readonly Label _healthLabel;
    private readonly Castle _playerCastle;
    private readonly Castle _enemyCastle;
    private Building _hoveredBuilding;
    private bool _inputBlocked;
    private bool _shopOpen;
    private bool _placementActive;
    private bool _pauseOpen;

    public BuildingInfoUiController(Node owner, CanvasLayer uiRoot)
    {
        _owner = owner;
        _panel = uiRoot.GetNode<Panel>("BuildingInfoPanel");
        _nameLabel = uiRoot.GetNode<Label>("BuildingInfoPanel/NameLabel");
        _healthLabel = uiRoot.GetNode<Label>("BuildingInfoPanel/HealthLabel");

        Node mainGame = owner.GetParent();
        _playerCastle = mainGame.GetNode<Castle>("Battlefield/PlayerSide/PlayerCastle");
        _enemyCastle = mainGame.GetNode<Castle>("Battlefield/EnemySide/EnemyCastle");

        HidePanel();
    }

    public void SetInputBlocked(bool blocked)
    {
        _inputBlocked = blocked;
        if (blocked)
            ClearHover();
    }

    public void SetShopOpen(bool open)
    {
        _shopOpen = open;
        if (open)
            ClearHover();
    }

    public void SetPlacementActive(bool active)
    {
        _placementActive = active;
        if (active)
            ClearHover();
    }

    public void SetPauseOpen(bool open)
    {
        _pauseOpen = open;
        if (open)
            ClearHover();
    }

    public void Process()
    {
        if (_inputBlocked || _shopOpen || _placementActive || _pauseOpen)
            return;

        Vector2 mouseGlobal = _owner.GetViewport().GetMousePosition();
        Building building = PickBuilding(mouseGlobal);
        if (building == null)
        {
            ClearHover();
            return;
        }

        if (building == _hoveredBuilding)
        {
            RefreshPanel(building);
            return;
        }

        _hoveredBuilding = building;
        RefreshPanel(building);
        UpdateHighlights(building);
    }

    private Building PickBuilding(Vector2 globalPoint)
    {
        if (_playerCastle.TryGetBuildingAtGlobalPoint(globalPoint, out Building building))
            return building;

        if (_enemyCastle.TryGetBuildingAtGlobalPoint(globalPoint, out building))
            return building;

        return null;
    }

    private void RefreshPanel(Building building)
    {
        _panel.Visible = true;
        _nameLabel.Text = building.DisplayName;
        _healthLabel.Text = $"生命：{building.Health}/{building.MaxHealth}";
    }

    private void UpdateHighlights(Building hovered)
    {
        _playerCastle.ClearAdjacencyHighlight();
        _enemyCastle.ClearAdjacencyHighlight();

        Castle castle = hovered.GetCastle();
        if (castle == null)
            return;

        IReadOnlyList<Building> targets = AdjacentSystem.Instance?.GetAdjacencyEffectTargets(hovered)
            ?? System.Array.Empty<Building>();
        castle.SetAdjacencyHighlightBuildings(targets);
    }

    private void ClearHover()
    {
        if (_hoveredBuilding == null && !_panel.Visible)
            return;

        _hoveredBuilding = null;
        HidePanel();
        _playerCastle.ClearAdjacencyHighlight();
        _enemyCastle.ClearAdjacencyHighlight();
    }

    private void HidePanel()
    {
        _panel.Visible = false;
    }
}
