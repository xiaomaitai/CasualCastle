// 单位属性面板：生命条、抗性、攻击、护甲、费用与技能展示
using CasualCastle.Adapters.Persistence;
using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Battle;
using CasualCastle.Domain.Building;
using Godot;
using System.Collections.Generic;

public sealed class AttributePanelController
{
	private const string PlaceholderTexture = "res://assets/art/placeholders/test_image.png";
	private const string HolyElementId = "holy";
	private const float NeutralMultiplier = 1f;
	private const float SectionIconX = 14f;
	private const float ResistanceIconY = 98f;
	private const float AttackIconY = 120f;
	private const float ArmorIconY = 142f;
	private const float CostIconY = 164f;
	private const string NoResistanceText = "无";
	private const string NoResistanceTooltip = "无有效抗性";
	private const string NoBonusTooltip = "无特殊修正";
	private static readonly Color HealthFillColor = new(0.24f, 0.72f, 0.34f, 1f);

	private readonly Panel _panel;
	private readonly TextureRect _portraitRect;
	private readonly Label _nameLabel;
	private readonly Label _subLabel;
	private readonly ProgressBar _healthBar;
	private readonly Label _healthValueLabel;
	private readonly Label _statsLabel;
	private readonly StatIconView _resistanceIcon;
	private readonly Label _resistanceValueLabel;
	private readonly StatIconView _attackIcon;
	private readonly Label _attackValueLabel;
	private readonly Label _attackTypeLabel;
	private readonly StatIconView _armorIcon;
	private readonly Label _armorValueLabel;
	private readonly Label _armorTypeLabel;
	private readonly StatIconView _costIcon;
	private readonly Label _costValueLabel;
	private readonly HBoxContainer _skillsRow;
	private readonly HBoxContainer _elementRow;
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
		_healthBar = uiRoot.GetNode<ProgressBar>("AttributePanel/HealthBar");
		_healthValueLabel = uiRoot.GetNode<Label>("AttributePanel/HealthValueLabel");
		_statsLabel = uiRoot.GetNode<Label>("AttributePanel/StatsLabel");
		_resistanceValueLabel = uiRoot.GetNode<Label>("AttributePanel/ResistanceValueLabel");
		_attackValueLabel = uiRoot.GetNode<Label>("AttributePanel/AttackValueLabel");
		_attackTypeLabel = uiRoot.GetNode<Label>("AttributePanel/AttackTypeLabel");
		_armorValueLabel = uiRoot.GetNode<Label>("AttributePanel/ArmorValueLabel");
		_armorTypeLabel = uiRoot.GetNode<Label>("AttributePanel/ArmorTypeLabel");
		_costValueLabel = uiRoot.GetNode<Label>("AttributePanel/CostValueLabel");
		_skillsRow = uiRoot.GetNode<HBoxContainer>("AttributePanel/SkillsRow");
		_elementRow = uiRoot.GetNode<HBoxContainer>("AttributePanel/ElementRow");
		_resistanceIcon = CreateStatIcon(StatIconView.StatIconKind.Resistance, ResistanceIconY, "抗性");
		_attackIcon = CreateStatIcon(StatIconView.StatIconKind.Attack, AttackIconY, "攻击");
		_armorIcon = CreateStatIcon(StatIconView.StatIconKind.Armor, ArmorIconY, "护甲");
		_costIcon = CreateStatIcon(StatIconView.StatIconKind.Cost, CostIconY, "费用");
		_healthBar.ShowPercentage = false;
		StyleBoxFlat fill = new() { BgColor = HealthFillColor };
		_healthBar.AddThemeStyleboxOverride("fill", fill);
		_panel.Visible = false;
	}

	private StatIconView CreateStatIcon(StatIconView.StatIconKind kind, float y, string tooltip)
	{
		StatIconView icon = new();
		icon.Configure(kind);
		icon.Position = new Vector2(SectionIconX, y);
		icon.TooltipText = tooltip;
		_panel.AddChild(icon);
		return icon;
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
			UpdateHealth(unit, null);
			_panel.Visible = true;
			return;
		}

		if (building != null && !building.IsDestroyed)
		{
			if (_currentBuilding != building)
				RebuildBuildingPanel(building);
			UpdateHealth(null, building);
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
		UpdateHeader(state.TypeId, unit.IsPlayerUnit, state.Race, state.IsStealthed, state.IsAmbushing);
		ShowUnitSections();
		UpdateResistanceArea(state);
		UpdateAttackArea(state);
		UpdateArmorArea(state);
		UpdateCostArea(state);
		UpdateSkills(state);
	}

	private void UpdateHeader(string typeId, bool isPlayerUnit, string race, bool isStealthed, bool isAmbushing)
	{
		_portraitRect.Texture = LoadUnitPortrait(typeId) ?? LoadPlaceholder();
		_nameLabel.Text = UnitDisplayName.Get(typeId);
		_subLabel.Text = (isPlayerUnit ? "我方" : "敌方")
			+ " · " + RaceName(race)
			+ (isStealthed ? " · 隐身中" : "")
			+ (isAmbushing ? " · 伏击中" : "");
	}

	private void ShowUnitSections()
	{
		_statsLabel.Visible = false;
		_resistanceIcon.Visible = true;
		_resistanceValueLabel.Visible = true;
		_attackIcon.Visible = true;
		_attackValueLabel.Visible = true;
		_attackTypeLabel.Visible = true;
		_armorIcon.Visible = true;
		_armorValueLabel.Visible = true;
		_armorTypeLabel.Visible = true;
		_costIcon.Visible = true;
		_costValueLabel.Visible = true;
		_elementRow.Visible = true;
	}

	private void ShowBuildingSections()
	{
		_statsLabel.Visible = true;
		_resistanceIcon.Visible = false;
		_resistanceValueLabel.Visible = false;
		_attackIcon.Visible = false;
		_attackValueLabel.Visible = false;
		_attackTypeLabel.Visible = false;
		_armorIcon.Visible = false;
		_armorValueLabel.Visible = false;
		_armorTypeLabel.Visible = false;
		_costIcon.Visible = false;
		_costValueLabel.Visible = false;
		_elementRow.Visible = false;
	}

	private void UpdateHealth(UnitLogic unit, Building building)
	{
		int health = unit != null ? unit.Handle.Health : building.Health;
		int maxHealth = unit != null ? unit.Handle.MaxHealth : building.MaxHealth;
		_healthBar.MaxValue = maxHealth;
		_healthBar.Value = health;
		_healthValueLabel.Text = $"{health}/{maxHealth}";
	}

	private void UpdateResistanceArea(IUnitState state)
	{
		List<ResistanceEntry> resistances = GetUnitResistances(state);
		_resistanceValueLabel.Text = BuildResistanceSummary(resistances);
		string tooltip = BuildResistanceTooltip(resistances);
		_resistanceIcon.TooltipText = tooltip;
		_resistanceValueLabel.TooltipText = tooltip;
	}

	private void UpdateAttackArea(IUnitState state)
	{
		_attackValueLabel.Text = state.Damage.ToString();
		_attackValueLabel.TooltipText = BuildAttackValueTooltip(state);
		_attackTypeLabel.Text = AttackName(state.AttackType) + "·" + DamageName(state.DamageType);
		_attackTypeLabel.TooltipText = BuildAttackTypeTooltip(state);
		UpdateElementRow(GetAttackElements(state));
	}

	private void UpdateArmorArea(IUnitState state)
	{
		_armorValueLabel.Text = state.EffectiveArmorValue.ToString();
		_armorValueLabel.TooltipText = BuildArmorValueTooltip(state);
		_armorTypeLabel.Text = ArmorName(state.ArmorType);
		_armorTypeLabel.TooltipText = BuildArmorTypeTooltip(state);
	}

	private void UpdateCostArea(IUnitState state)
	{
		_costValueLabel.Text = state.UnitCost.ToString();
	}

	private void UpdateSkills(IUnitState state)
	{
		ClearSkillsRow();
		foreach (SkillDef skill in state.Skills.All)
		{
			SkillIconView icon = new();
			icon.Configure(skill);
			_skillsRow.AddChild(icon);
			_skillIcons.Add(icon);
		}
	}

	private void UpdateElementRow(IReadOnlyList<string> elementIds)
	{
		ClearElementRow();
		foreach (string elementId in elementIds)
		{
			ElementIconView icon = new();
			icon.Configure(elementId);
			_elementRow.AddChild(icon);
		}
	}

	private void RebuildBuildingPanel(Building building)
	{
		_currentBuilding = building;
		_currentUnit = null;
		Sprite2D sprite = building.GetNodeOrNull<Sprite2D>("View/Sprite");
		_portraitRect.Texture = sprite?.Texture ?? LoadPlaceholder();
		_nameLabel.Text = building.DisplayName;
		_subLabel.Text = (building.IsPlayerOwned ? "我方" : "敌方") + " · " + GetBuildingStatus(building);
		ShowBuildingSections();
		_statsLabel.Text = BuildBuildingStats(building);
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

	private static string BuildBuildingStats(Building building)
	{
		BuildingSystem system = AdapterRegistry.Resolve<BuildingSystem>();
		string type = system.IsCoreBuilding(building.TypeId) ? "核心建筑" : "普通建筑";
		string lines = $"占地 {system.GetFootprint(building.TypeId).Count} 格\n"
			+ $"类型 {type}   价值 {system.GetValue(building.TypeId)}\n";

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
		return building.CanWork ? "工作中" : "休眠";
	}

	private static string BuildResistanceSummary(IReadOnlyList<ResistanceEntry> resistances)
	{
		if (resistances.Count == 0)
			return NoResistanceText;
		ResistanceEntry strongest = resistances[0];
		ResistanceEntry weakest = resistances[0];
		foreach (ResistanceEntry entry in resistances)
		{
			if (entry.Percent > strongest.Percent)
				strongest = entry;
			if (entry.Percent < weakest.Percent)
				weakest = entry;
		}
		return $"最强 {strongest.Name} {strongest.Percent}%   最弱 {weakest.Name} {weakest.Percent}%";
	}

	private static string BuildResistanceTooltip(IReadOnlyList<ResistanceEntry> resistances)
	{
		if (resistances.Count == 0)
			return NoResistanceTooltip;
		List<string> lines = new();
		foreach (ResistanceEntry entry in resistances)
			lines.Add($"{entry.Name} {entry.Percent}%");
		return string.Join("\n", lines);
	}

	private static string BuildAttackValueTooltip(IUnitState state)
	{
		return $"攻击值范围：{state.Damage} ~ {state.Damage}";
	}

	private static string BuildAttackTypeTooltip(IUnitState state)
	{
		DamageMatrix damageMatrix = GameManager.Get<DamageMatrix>();
		List<string> lines = new();
		foreach (ArmorType armor in System.Enum.GetValues<ArmorType>())
		{
			float multiplier = damageMatrix.GetMultiplier(state.DamageType, armor);
			if (!Mathf.IsEqualApprox(multiplier, NeutralMultiplier))
				lines.Add($"{ArmorName(armor)} {FormatMultiplier(multiplier)}");
		}
		if (lines.Count == 0)
			return NoBonusTooltip;
		return "对护甲伤害修正\n" + string.Join("\n", lines);
	}

	private static string BuildArmorValueTooltip(IUnitState state)
	{
		double reduction = DamageMatrix.CalculateReduction(state.EffectiveArmorValue);
		int percent = (int)System.Math.Round(reduction * DamageMatrix.PercentScale);
		return $"护甲减伤：{percent}%";
	}

	private static string BuildArmorTypeTooltip(IUnitState state)
	{
		DamageMatrix damageMatrix = GameManager.Get<DamageMatrix>();
		List<string> lines = new();
		foreach (DamageType damage in System.Enum.GetValues<DamageType>())
		{
			float multiplier = damageMatrix.GetMultiplier(damage, state.ArmorType);
			if (!Mathf.IsEqualApprox(multiplier, NeutralMultiplier))
				lines.Add($"{DamageName(damage)} {FormatMultiplier(multiplier)}");
		}
		if (lines.Count == 0)
			return NoBonusTooltip;
		return "受到攻击伤害修正\n" + string.Join("\n", lines);
	}

	private static string FormatMultiplier(float multiplier)
	{
		int percent = Mathf.RoundToInt((multiplier - NeutralMultiplier) * DamageMatrix.PercentScale);
		return percent > 0 ? $"+{percent}%" : $"{percent}%";
	}

	private static IReadOnlyList<string> GetAttackElements(IUnitState state)
	{
		List<string> elements = new();
		if (state.DamageType == DamageType.Holy)
			elements.Add(HolyElementId);
		return elements;
	}

	private static List<ResistanceEntry> GetUnitResistances(IUnitState state)
	{
		return new List<ResistanceEntry>();
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

	private void ClearElementRow()
	{
		foreach (Node child in _elementRow.GetChildren())
		{
			_elementRow.RemoveChild(child);
			child.QueueFree();
		}
	}

	public static Texture2D LoadUnitPortrait(string typeId)
	{
		IVisualAssetRepository repository = GameManager.Get<IVisualAssetRepository>();
		string portraitUid = repository?.GetPortraitUid(typeId) ?? "";
		if (string.IsNullOrEmpty(portraitUid))
			return null;
		return GD.Load<Texture2D>(portraitUid);
	}

	public static Texture2D LoadPlaceholder()
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

	private static string AttackName(AttackType attackType)
	{
		return attackType == AttackType.Ranged ? "远程" : "近战";
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
			"Orc" => "兽人",
			"Goblin" => "哥布林",
			"Centaur" => "半人马",
			"Staglur" => "半人鹿",
			"Lizard" => "蜥蜴人",
			"Undead" => "亡灵",
			"Demon" => "恶魔",
			"DemonKing" => "魔王",
			_ => race,
		};
	}

	private readonly struct ResistanceEntry
	{
		public readonly string Name;
		public readonly int Percent;
		public ResistanceEntry(string name, int percent)
		{
			Name = name;
			Percent = percent;
		}
	}
}
