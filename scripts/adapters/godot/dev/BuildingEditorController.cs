// 建筑编辑器：建筑定义编辑与派系库存管理
using System.Collections.Generic;
using System.Linq;
using CasualCastle.Domain.Building;
using Godot;

namespace CasualCastle.Adapters.Godot.Dev;

public sealed class BuildingEditorController
{
	private const string DevToolsMenuScene = "res://scenes/ui/dev_tools/dev_tools_menu.tscn";
	private const string TechTreeEditorScene = "res://scenes/dev/tech_tree_editor.tscn";
	private const int DefaultBuildingValue = 1;
	private const int DefaultMaxHealth = 200;
	private const float DefaultSpawnInterval = 10.0f;
	private const int DefaultShopWeight = 1;
	private const int DefaultCollisionSize = 188;
	private const float ListItemWidth = 220f;
	private const float ListItemHeight = 32f;

	private readonly Control _root;
	private readonly Button _prevFactionButton;
	private readonly Label _factionNameLabel;
	private readonly Button _nextFactionButton;
	private readonly Button _techTreeButton;
	private readonly Button _closeButton;
	private readonly VBoxContainer _buildingList;
	private readonly Button _newBuildingButton;
	private readonly LineEdit _typeIdEdit;
	private readonly LineEdit _nameEdit;
	private readonly SpinBox _valueSpin;
	private readonly SpinBox _healthSpin;
	private readonly SpinBox _spawnSpin;
	private readonly LineEdit _unitTypeEdit;
	private readonly CheckBox _shopCheck;
	private readonly SpinBox _costSpin;
	private readonly SpinBox _weightSpin;
	private readonly SpinBox _collisionWidthSpin;
	private readonly SpinBox _collisionHeightSpin;
	private readonly Button _saveButton;
	private readonly Button _addToInventoryButton;
	private readonly Button _removeFromInventoryButton;
	private readonly Label _inventoryStatusLabel;
	private readonly Label _statusBar;

	private readonly ITechTreeRepository _repo;
	private List<FactionDef> _factions = new();
	private List<BuildingTypeSummary> _buildings = new();
	private string _currentFactionId = "";
	private int _currentFactionIndex = 0;
	private string _selectedTypeId = "";
	private bool _isNewBuilding = false;

	public BuildingEditorController(Control root, ITechTreeRepository repo)
	{
		_root = root;
		_prevFactionButton = root.GetNode<Button>("TopBar/PrevFactionButton");
		_factionNameLabel = root.GetNode<Label>("TopBar/FactionNameLabel");
		_nextFactionButton = root.GetNode<Button>("TopBar/NextFactionButton");
		_techTreeButton = root.GetNode<Button>("TopBar/TechTreeButton");
		_closeButton = root.GetNode<Button>("TopBar/CloseButton");
		_buildingList = root.GetNode<VBoxContainer>("EditorArea/BuildingListPanel/BuildingScroll/BuildingList");
		_newBuildingButton = root.GetNode<Button>("EditorArea/BuildingListPanel/NewBuildingButton");
		_typeIdEdit = root.GetNode<LineEdit>("EditorArea/EditPanel/TypeIdRow/TypeIdEdit");
		_nameEdit = root.GetNode<LineEdit>("EditorArea/EditPanel/NameRow/NameEdit");
		_valueSpin = root.GetNode<SpinBox>("EditorArea/EditPanel/ValueRow/ValueSpin");
		_healthSpin = root.GetNode<SpinBox>("EditorArea/EditPanel/HealthRow/HealthSpin");
		_spawnSpin = root.GetNode<SpinBox>("EditorArea/EditPanel/SpawnRow/SpawnSpin");
		_unitTypeEdit = root.GetNode<LineEdit>("EditorArea/EditPanel/UnitTypeRow/UnitTypeEdit");
		_shopCheck = root.GetNode<CheckBox>("EditorArea/EditPanel/ShopRow/ShopCheck");
		_costSpin = root.GetNode<SpinBox>("EditorArea/EditPanel/CostRow/CostSpin");
		_weightSpin = root.GetNode<SpinBox>("EditorArea/EditPanel/WeightRow/WeightSpin");
		_collisionWidthSpin = root.GetNode<SpinBox>("EditorArea/EditPanel/CollisionWidthRow/CollisionWidthSpin");
		_collisionHeightSpin = root.GetNode<SpinBox>("EditorArea/EditPanel/CollisionHeightRow/CollisionHeightSpin");
		_saveButton = root.GetNode<Button>("EditorArea/EditPanel/SaveButton");
		_addToInventoryButton = root.GetNode<Button>("EditorArea/EditPanel/InventoryRow/AddToInventoryButton");
		_removeFromInventoryButton = root.GetNode<Button>("EditorArea/EditPanel/InventoryRow/RemoveFromInventoryButton");
		_inventoryStatusLabel = root.GetNode<Label>("EditorArea/EditPanel/InventoryStatusLabel");
		_statusBar = root.GetNode<Label>("StatusBar");

		_repo = repo;

		_spawnSpin.Step = 0.5;
		_weightSpin.Value = DefaultShopWeight;

		_prevFactionButton.Pressed += OnPrevFactionPressed;
		_nextFactionButton.Pressed += OnNextFactionPressed;
		_techTreeButton.Pressed += OnTechTreePressed;
		_closeButton.Pressed += OnClosePressed;
		_newBuildingButton.Pressed += OnNewBuildingPressed;
		_saveButton.Pressed += OnSavePressed;
		_addToInventoryButton.Pressed += OnAddToInventoryPressed;
		_removeFromInventoryButton.Pressed += OnRemoveFromInventoryPressed;
	}

	public void Dispose()
	{
		_prevFactionButton.Pressed -= OnPrevFactionPressed;
		_nextFactionButton.Pressed -= OnNextFactionPressed;
		_techTreeButton.Pressed -= OnTechTreePressed;
		_closeButton.Pressed -= OnClosePressed;
		_newBuildingButton.Pressed -= OnNewBuildingPressed;
		_saveButton.Pressed -= OnSavePressed;
		_addToInventoryButton.Pressed -= OnAddToInventoryPressed;
		_removeFromInventoryButton.Pressed -= OnRemoveFromInventoryPressed;
	}

	public void LoadInitialData()
	{
		_factions = _repo.LoadFactions();
		if (_factions.Count == 0)
			return;

		int startIndex = 0;
		if (!string.IsNullOrEmpty(DevEditorContext.CurrentFactionId))
		{
			int contextIndex = _factions.FindIndex(f => f.Id == DevEditorContext.CurrentFactionId);
			if (contextIndex >= 0)
				startIndex = contextIndex;
		}
		LoadFactionAtIndex(startIndex);
	}

	private void OnPrevFactionPressed()
	{
		if (_currentFactionIndex > 0)
			LoadFactionAtIndex(_currentFactionIndex - 1);
	}

	private void OnNextFactionPressed()
	{
		if (_currentFactionIndex < _factions.Count - 1)
			LoadFactionAtIndex(_currentFactionIndex + 1);
	}

	private void LoadFactionAtIndex(int index)
	{
		_currentFactionIndex = index;
		FactionDef faction = _factions[index];
		_currentFactionId = faction.Id;
		_factionNameLabel.Text = faction.DisplayName;
		_buildings = _repo.LoadAllBuildingTypes(faction.Id);
		RefreshBuildingList();
		ClearForm();
		ShowStatus($"已加载派系: {faction.DisplayName}");
	}

	private void RefreshBuildingList()
	{
		for (int i = _buildingList.GetChildCount() - 1; i >= 0; i--)
		{
			Node child = _buildingList.GetChild(i);
			_buildingList.RemoveChild(child);
			child.QueueFree();
		}

		foreach (BuildingTypeSummary building in _buildings)
		{
			bool inInventory = _repo.IsInInventory(_currentFactionId, building.TypeId);
			_buildingList.AddChild(CreateBuildingListItem(building, inInventory));
		}
	}

	private Panel CreateBuildingListItem(BuildingTypeSummary building, bool inInventory)
	{
		Panel panel = new()
		{
			CustomMinimumSize = new Vector2(ListItemWidth, ListItemHeight),
			Size = new Vector2(ListItemWidth, ListItemHeight),
			MouseFilter = Control.MouseFilterEnum.Stop
		};

		StyleBoxFlat styleBox = new();
		styleBox.BgColor = inInventory
			? new Color(0.2f, 0.4f, 0.55f)
			: new Color(0.35f, 0.3f, 0.3f);
		panel.AddThemeStyleboxOverride("panel", styleBox);

		Label label = new()
		{
			Text = $"{building.DisplayName}（价值 {building.Value}）",
			Position = new Vector2(6, 6),
			Size = new Vector2(ListItemWidth - 12, ListItemHeight - 12),
			Modulate = new Color(1f, 1f, 1f)
		};
		panel.AddChild(label);

		if (inInventory)
		{
			Label badge = new()
			{
				Text = "库",
				Position = new Vector2(ListItemWidth - 28, 4),
				Size = new Vector2(22, 16),
				HorizontalAlignment = HorizontalAlignment.Center,
				Modulate = new Color(1f, 0.9f, 0.4f)
			};
			panel.AddChild(badge);
		}

		panel.SetMeta("type_id", building.TypeId);
		panel.GuiInput += e => OnBuildingListItemInput(panel, e);

		return panel;
	}

	private void OnBuildingListItemInput(Panel item, InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseButton && mouseButton.Pressed && mouseButton.ButtonIndex == MouseButton.Left)
		{
			SelectBuilding(item.GetMeta("type_id").AsString());
			item.AcceptEvent();
		}
	}

	private void SelectBuilding(string typeId)
	{
		_selectedTypeId = typeId;
		_isNewBuilding = false;
		BuildingEditData data = _repo.LoadBuilding(typeId);
		PopulateForm(data);
		_typeIdEdit.Editable = false;
		UpdateInventoryStatus();
	}

	private void ClearForm()
	{
		_selectedTypeId = "";
		_isNewBuilding = false;
		_typeIdEdit.Text = "";
		_typeIdEdit.Editable = true;
		_nameEdit.Text = "";
		_valueSpin.Value = DefaultBuildingValue;
		_healthSpin.Value = DefaultMaxHealth;
		_spawnSpin.Value = DefaultSpawnInterval;
		_unitTypeEdit.Text = "";
		_shopCheck.ButtonPressed = false;
		_costSpin.Value = 0;
		_weightSpin.Value = DefaultShopWeight;
		_collisionWidthSpin.Value = DefaultCollisionSize;
		_collisionHeightSpin.Value = DefaultCollisionSize;
		_inventoryStatusLabel.Text = "未选择建筑";
	}

	private void PopulateForm(BuildingEditData data)
	{
		_typeIdEdit.Text = data.TypeId;
		_nameEdit.Text = data.DisplayName;
		_valueSpin.Value = data.Value;
		_healthSpin.Value = data.MaxHealth;
		_spawnSpin.Value = data.SpawnInterval;
		_unitTypeEdit.Text = data.UnitTypeId ?? "";
		_shopCheck.ButtonPressed = data.ShopAvailable;
		_costSpin.Value = data.GoldCost;
		_weightSpin.Value = data.ShopWeight;
		_collisionWidthSpin.Value = data.CollisionWidth;
		_collisionHeightSpin.Value = data.CollisionHeight;
	}

	private void OnNewBuildingPressed()
	{
		_isNewBuilding = true;
		_selectedTypeId = "";
		_typeIdEdit.Text = "";
		_typeIdEdit.Editable = true;
		_nameEdit.Text = "新建筑";
		_valueSpin.Value = DefaultBuildingValue;
		_healthSpin.Value = DefaultMaxHealth;
		_spawnSpin.Value = DefaultSpawnInterval;
		_unitTypeEdit.Text = "";
		_shopCheck.ButtonPressed = false;
		_costSpin.Value = 0;
		_weightSpin.Value = DefaultShopWeight;
		_collisionWidthSpin.Value = DefaultCollisionSize;
		_collisionHeightSpin.Value = DefaultCollisionSize;
		_inventoryStatusLabel.Text = "未加入库存";
		_typeIdEdit.GrabFocus();
		ShowStatus("新建建筑：填写 TypeID 与名称后保存");
	}

	private void OnSavePressed()
	{
		string typeId = _typeIdEdit.Text.Trim();
		string displayName = _nameEdit.Text.Trim();
		if (string.IsNullOrEmpty(typeId) || string.IsNullOrEmpty(displayName))
		{
			ShowStatus("TypeID 和名称不能为空");
			return;
		}

		BuildingEditData data = new()
		{
			TypeId = typeId,
			DisplayName = displayName,
			FactionId = _currentFactionId,
			MaxHealth = (int)_healthSpin.Value,
			SpawnInterval = (float)_spawnSpin.Value,
			Value = (int)_valueSpin.Value,
			UnitTypeId = _unitTypeEdit.Text.Trim(),
			ShopAvailable = _shopCheck.ButtonPressed,
			GoldCost = (int)_costSpin.Value,
			ShopWeight = (int)_weightSpin.Value,
			CollisionWidth = (int)_collisionWidthSpin.Value,
			CollisionHeight = (int)_collisionHeightSpin.Value
		};
		_repo.SaveBuilding(data);

		if (_isNewBuilding)
		{
			_isNewBuilding = false;
			_selectedTypeId = typeId;
			_typeIdEdit.Editable = false;
		}

		_buildings = _repo.LoadAllBuildingTypes(_currentFactionId);
		RefreshBuildingList();
		UpdateInventoryStatus();
		ShowStatus($"已保存: {displayName}");
	}

	private void OnAddToInventoryPressed()
	{
		if (string.IsNullOrEmpty(_selectedTypeId))
		{
			ShowStatus("请先选择或保存一个建筑");
			return;
		}
		_repo.AddToInventory(_currentFactionId, _selectedTypeId);
		UpdateInventoryStatus();
		RefreshBuildingList();
		ShowStatus($"已加入 {_factionNameLabel.Text} 库存");
	}

	private void OnRemoveFromInventoryPressed()
	{
		if (string.IsNullOrEmpty(_selectedTypeId))
			return;
		_repo.RemoveFromInventory(_currentFactionId, _selectedTypeId);
		UpdateInventoryStatus();
		RefreshBuildingList();
		ShowStatus("已从库存移除");
	}

	private void UpdateInventoryStatus()
	{
		if (string.IsNullOrEmpty(_selectedTypeId))
		{
			_inventoryStatusLabel.Text = "未选择建筑";
			return;
		}
		bool inInventory = _repo.IsInInventory(_currentFactionId, _selectedTypeId);
		_inventoryStatusLabel.Text = inInventory ? "已在本派系库存中" : "不在本派系库存中";
	}

	private void OnTechTreePressed()
	{
		DevEditorContext.CurrentFactionId = _currentFactionId;
		_root.GetTree().ChangeSceneToFile(TechTreeEditorScene);
	}

	private void OnClosePressed()
	{
		_root.GetTree().ChangeSceneToFile(DevToolsMenuScene);
	}

	private async void ShowStatus(string message)
	{
		_statusBar.Text = message;
		await _root.ToSignal(_root.GetTree().CreateTimer(3), "timeout");
		_statusBar.Text = "就绪";
	}
}
