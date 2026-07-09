using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Building;
using Godot;
using System.Collections.Generic;

public sealed class BuildingInfoUiController
{
    private readonly Node _owner;
    private readonly Panel _panel;
    private readonly Label _nameLabel;
    private readonly Label _healthLabel;
    private readonly Label _statusLabel;
    private readonly Castle _playerCastle;
    private readonly Castle _enemyCastle;
    private Building _hoveredBuilding;
    private bool _inputBlocked;
    private bool _placementActive;
    private bool _pauseOpen;

    public BuildingInfoUiController(Node owner, CanvasLayer uiRoot)
    {
        _owner = owner;
        _panel = uiRoot.GetNode<Panel>("BuildingInfoPanel");
        _nameLabel = uiRoot.GetNode<Label>("BuildingInfoPanel/NameLabel");
        _healthLabel = uiRoot.GetNode<Label>("BuildingInfoPanel/HealthLabel");
        _statusLabel = uiRoot.GetNode<Label>("BuildingInfoPanel/StatusLabel");

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
        if (_inputBlocked || _placementActive || _pauseOpen)
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
        _statusLabel.Text = GetStatusText(building);
    }

    private static string GetStatusText(Building building)
    {
        if (building.IsDestroyed)
            return "已摧毁";
        if (building.IsManuallyPaused)
            return "手动暂停";
        if (building.IsFusionProhibited)
            return "已禁止融合";
        if (GameManager.Instance?.IsNight == true)
            return building.HasNightCombat ? "夜晚可行动" : "夜晚休眠";
        return building.CanWork ? "工作中" : "停止工作";
    }

    private void UpdateHighlights(Building hovered)
    {
        _playerCastle.ClearAdjacencyHighlight();
        _enemyCastle.ClearAdjacencyHighlight();

        Castle castle = hovered.GetCastle();
        if (castle == null)
            return;

        AdjacencyService adjacencyService = GameManager.Get<AdjacencyService>();
        IReadOnlyList<IAdjacencyBuilding> domainTargets = adjacencyService.GetAdjacentSameTypeTargets(hovered, castle.GetBuildingStates());
        List<Building> targets = new();
        foreach (IAdjacencyBuilding t in domainTargets)
        {
            if (t is Building b)
                targets.Add(b);
        }
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
