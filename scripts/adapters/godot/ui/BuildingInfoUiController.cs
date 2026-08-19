// 建筑信息组件：悬停详情（building_info_panel.tscn 场景根）
using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Building;
using Godot;
using System.Collections.Generic;

public partial class BuildingInfoUiController : Control
{
	private Panel _panel;
	private Label _nameLabel;
	private Label _healthLabel;
	private Label _statusLabel;
	private Castle _playerCastle;
	private Castle _enemyCastle;
	private Building _hoveredBuilding;
	private bool _inputBlocked;
	private HandUiController _handUi;
	private ShopUiController _shopUi;
	private PauseMenuUiController _pauseMenuUi;
	private SettingsUiController _settingsUi;

	private HandUiController HandUi => _handUi ??= AdapterRegistry.Resolve<HandUiController>();
	private ShopUiController ShopUi => _shopUi ??= AdapterRegistry.Resolve<ShopUiController>();
	private PauseMenuUiController PauseMenu => _pauseMenuUi ??= AdapterRegistry.Resolve<PauseMenuUiController>();
	private SettingsUiController Settings => _settingsUi ??= AdapterRegistry.Resolve<SettingsUiController>();

	public override void _Ready()
	{
		_panel = GetNode<Panel>("BuildingInfoPanel");
		_nameLabel = GetNode<Label>("BuildingInfoPanel/NameLabel");
		_healthLabel = GetNode<Label>("BuildingInfoPanel/HealthLabel");
		_statusLabel = GetNode<Label>("BuildingInfoPanel/StatusLabel");

		Node mainGame = GetParent().GetParent();
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

	public override void _Process(double _)
	{
		if (_inputBlocked || IsPlacementActive() || IsPauseOpen())
			return;

		Vector2 mouseScreen = GetViewport().GetMousePosition();
		Building building = PickBuilding(mouseScreen);
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

	private bool IsPlacementActive()
	{
		return HandUi?.IsPlacementActive == true || ShopUi?.IsDragging == true;
	}

	private bool IsPauseOpen()
	{
		return PauseMenu?.IsOpen == true || Settings?.IsOpen == true || ShopUi?.IsOpen == true;
	}

	private Building PickBuilding(Vector2 screenPoint)
	{
		if (_playerCastle.TryGetBuildingAtScreenPoint(screenPoint, out Building building))
			return building;

		if (_enemyCastle.TryGetBuildingAtScreenPoint(screenPoint, out building))
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
		if (building.IsCombineProhibited)
			return "已禁止组合";
		return building.CanWork ? "工作中" : "休眠";
	}

	private void UpdateHighlights(Building hovered)
	{
		_playerCastle.ClearAdjacencyHighlight();
		_enemyCastle.ClearAdjacencyHighlight();

		Castle castle = hovered.GetCastle();
		if (castle == null)
			return;

		if (GameManager.Get<IBuildingRepository>().GetFactionId(hovered.TypeId) == FactionDef.HordeId)
			return;

		AdjacencyService adjacencyService = GameManager.Get<AdjacencyService>();
		IReadOnlyList<IAdjacencyBuilding> domainTargets = adjacencyService.GetAdjacentSameTypeTargets(hovered.Bridge, castle.GetBuildingStates());
		List<Building> targets = new();
		foreach (IAdjacencyBuilding t in domainTargets)
		{
			if (t is BuildingBridge b)
				targets.Add(b.Building);
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