using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Battle;
using CasualCastle.Domain.Building;
using Godot;
using System.Collections.Generic;

public sealed class AttributePanelController
{
	private const string PlaceholderTexture = "res://assets/art/placeholders/test_image.png";

	private readonly Panel _panel;
	private readonly TextureRect _portraitRect;
	private readonly Label _nameLabel;
	private readonly Label _subLabel;
	private readonly Label _statsLabel;
	private readonly Label _skillsLabel;
	private readonly HBoxContainer _skillsRow;
	private readonly UnitSelectionController _selection;
	private readonly List<SkillIconView> _skillIcons = new();
	private UnitLogic _currentUnit;
	private Building _currentBuilding;

	public AttributePanelController(Node owner, CanvasLayer uiRoot, UnitSelectionController selection)
	{
		_selection = selection;
		_panel = uiRoot.GetNode<Panel>("AttributePanel");
		_portraitRect = uiRoot.GetNode<TextureRect>("AttributePanel/PortraitRect");
		_nameLabel = uiRoot.GetNode<Label>("AttributePanel/NameLabel");
		_subLabel = uiRoot.GetNode<Label>("AttributePanel/SubLabel");
		_statsLabel = uiRoot.GetNode<Label>("AttributePanel/StatsLabel");
		_skillsLabel = uiRoot.GetNode<Label>("AttributePanel/SkillsLabel");
		_skillsRow = uiRoot.GetNode<HBoxContainer>("AttributePanel/SkillsRow");
		_panel.Visible = false;
	}

	public void Process()
	{
		UnitLogic unit = _selection.SelectedUnit;
		Building building = _selection.SelectedBuilding;

		if (unit != null && unit.IsAlive)
		{
			if (_currentUnit != unit)
				RebuildUnitPanel(unit);
			RefreshUnitCooldowns(unit);
			_panel.Visible = true;
			return;
		}

		if (building != null && !building.IsDestroyed)
		{
			if (_currentBuilding != building)
				RebuildBuildingPanel(building);
			_panel.Visible = true;
			return;
		}

		_panel.Visible = false;
		_currentUnit = null;
		_currentBuilding = null;
		ClearSkillsRow();
	}

	private void RebuildUnitPanel(UnitLogic unit)
	{
		_currentUnit = unit;
		_currentBuilding = null;
		IUnitState state = unit.Handle;

		_portraitRect.Texture = LoadUnitPortrait(state.TypeId) ?? LoadPlaceholder();
		_nameLabel.Text = UnitDisplayName.Get(state.TypeId);
		_subLabel.Text = (unit.IsPlayerUnit ? "我方" : "敌方")
			+ " · " + RaceName(state.Race)
			+ (state.IsStealthed ? " · 隐身中" : "");
		_statsLabel.Text = BuildUnitStats(state);

		ClearSkillsRow();
		foreach (SkillDef skill in state.Skills.All)
		{
			SkillIconView icon = new SkillIconView();
			icon.Configure(skill);
			_skillsRow.AddChild(icon);
			_skillIcons.Add(icon);
		}
		_skillsLabel.Visible = state.Skills.Count > 0;
	}

	private void RebuildBuildingPanel(Building building)
	{
		_currentBuilding = building;
		_currentUnit = null;

		Sprite2D sprite = building.GetNodeOrNull<Sprite2D>("View/Sprite");
		_portraitRect.Texture = sprite?.Texture ?? LoadPlaceholder();
		_nameLabel.Text = building.DisplayName;
		_subLabel.Text = (building.IsPlayerOwned ? "我方" : "敌方") + " · " + GetBuildingStatus(building);
		_statsLabel.Text = BuildBuildingStats(building);
		_skillsLabel.Visible = false;
		ClearSkillsRow();
	}

	private void RefreshUnitCooldowns(UnitLogic unit)
	{
		IUnitState state = unit.Handle;
		for (int i = 0; i < _skillIcons.Count && i < state.Skills.All.Count; i++)
		{
			SkillDef skill = state.Skills.All[i];
			SpecialConfig special = state.Skills.GetSpecialById(skill.Id);
			float total = special?.Cooldown ?? 0f;
			float remaining = state.GetSkillCooldownRemaining(skill.Id);
			_skillIcons[i].SetCooldown(remaining, total);
		}
	}

	private static string BuildUnitStats(IUnitState state)
	{
		string attack = state.AttackType == AttackType.Ranged ? "远程" : "近战";
		string status = state.IsStealthed ? "隐身中" : "正常";
		string shieldLine = state.ShieldHealth > 0 ? $"   护盾 {state.ShieldHealth}\n" : "";
		string healLine = state.HealAmount > 0f
			? $"治疗 {state.HealAmount:0}   范围 {state.HealRange:0}   间隔 {state.HealInterval:0.0}s\n"
			: "";
		return $"生命 {state.Health}/{state.MaxHealth}   伤害 {state.Damage}\n"
			+ $"速度 {state.Speed:0}   射程 {state.AttackRange:0}   冷却 {state.AttackCooldown:0.0}s\n"
			+ $"视野 {state.VisionRange:0}   攻击 {attack}·{DamageName(state.DamageType)}\n"
			+ $"护甲 {ArmorName(state.ArmorType)} {state.ArmorValue}   费用 {state.UnitCost}   状态 {status}\n"
			+ shieldLine + healLine;
	}

	private static string BuildBuildingStats(Building building)
	{
		BuildingSystem system = AdapterRegistry.Resolve<BuildingSystem>();
		string type = system.IsCoreBuilding(building.TypeId) ? "核心建筑" : "普通建筑";
		string lines = $"生命 {building.Health}/{building.MaxHealth}   占地 {system.GetFootprint(building.TypeId).Count} 格\n"
			+ $"类型 {type}\n";

		string unitTypeId = GameManager.Get<IBuildingRepository>().GetUnitTypeId(building.TypeId);
		if (string.IsNullOrEmpty(unitTypeId))
			return lines + "不产出单位";

		UnitStats stats = GameManager.Get<IUnitRepository>().Get(unitTypeId);
		return lines + $"生产 {UnitDisplayName.Get(stats.TypeId)}   成本 {stats.UnitCost}\n"
			+ $"生产率 {system.GetProductionRate(building.TypeId):0}/s";
	}

	private static string GetBuildingStatus(Building building)
	{
		if (building.IsDestroyed)
			return "已摧毁";
		if (building.IsManuallyPaused)
			return "手动暂停";
		if (building.IsCombineProhibited)
			return "已禁止组合";
		if (GameManager.Instance?.IsNight == true)
			return "夜晚休眠";
		return building.CanWork ? "工作中" : "停止工作";
	}

	private void ClearSkillsRow()
	{
		foreach (Node child in _skillsRow.GetChildren())
		{
			_skillsRow.RemoveChild(child);
			child.QueueFree();
		}
		_skillIcons.Clear();
	}

	private static Texture2D LoadUnitPortrait(string typeId)
	{
		using DirAccess dir = DirAccess.Open("res://assets/art/cards/");
		if (dir == null)
			return null;
		string prefix = typeId + "_";
		dir.ListDirBegin();
		string fileName = dir.GetNext();
		while (fileName != "")
		{
			if (fileName.StartsWith(prefix) && fileName.EndsWith(".png") && !dir.CurrentIsDir())
				return GD.Load<Texture2D>($"res://assets/art/cards/{fileName}");
			fileName = dir.GetNext();
		}
		return null;
	}

	private static Texture2D LoadPlaceholder()
	{
		return GD.Load<Texture2D>(PlaceholderTexture);
	}

	private static string ArmorName(ArmorType armorType)
	{
		return armorType switch
		{
			ArmorType.Unarmored => "无护甲",
			ArmorType.Cloth => "布甲",
			ArmorType.Leather => "皮甲",
			ArmorType.Chainmail => "锁甲",
			ArmorType.Plate => "板甲",
			ArmorType.Fortification => "城防",
			ArmorType.Holy => "神圣",
			_ => armorType.ToString(),
		};
	}

	private static string DamageName(DamageType damageType)
	{
		return damageType switch
		{
			DamageType.Slash => "劈砍",
			DamageType.Pierce => "穿刺",
			DamageType.Blunt => "钝击",
			DamageType.Siege => "攻城",
			DamageType.Magic => "魔法",
			DamageType.Holy => "神圣",
			DamageType.Chaos => "混沌",
			_ => damageType.ToString(),
		};
	}

	private static string RaceName(string race)
	{
		return race switch
		{
			"Human" => "人类",
			"Elf" => "精灵",
			"Dwarf" => "矮人",
			"Halfling" => "半身人",
			"Dragonborn" => "龙裔",
			_ => race,
		};
	}
}
