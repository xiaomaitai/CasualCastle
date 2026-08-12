// 左下角属性面板：展示单位与建筑的核心属性和实时进度
using CasualCastle.Adapters.Persistence;
using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Battle;
using CasualCastle.Domain.Building;
using Godot;
using System;
using System.Collections.Generic;

public sealed class AttributePanelController
{
	private const string PlaceholderTexture = "res://assets/art/placeholders/test_image.png";
	private const string HolyElementId = "holy";
	private const float NeutralMultiplier = 1f;
	private const float StatRowHeight = 20f;
	private const float StatColumnWidth = 198f;
	private const string NoBonusTooltip = "无特殊修正";

	private readonly Panel _panel;
	private readonly TextureRect _portraitRect;
	private readonly Label _nameLabel;
	private readonly Label _subLabel;
	private readonly Label _badgeLabel;
	private readonly ProgressBar _healthBar;
	private readonly Label _healthValueLabel;
	private readonly GridContainer _statsGrid;
	private readonly Control _bottomArea;
	private readonly HBoxContainer _skillsRow;
	private readonly VBoxContainer _progressBox;
	private readonly Label _progressLabel;
	private readonly ProgressBar _progressBar;
	private readonly UnitSelectionController _selection;
	private readonly List<SkillIconView> _skillIcons = new();
	private UnitLogic _currentUnit;
	private Building _currentBuilding;
	private Label _attackLabel;
	private Label _armorLabel;
	private Label _shieldLabel;
	private Label _productionLabel;
	private bool _shieldEntryVisible;

	public AttributePanelController(CanvasLayer uiRoot, UnitSelectionController selection)
	{
		_selection = selection;
		_panel = uiRoot.GetNode<Panel>("AttributePanel");
		_portraitRect = _panel.GetNode<TextureRect>("Margin/Content/Header/PortraitFrame/PortraitRect");
		_nameLabel = _panel.GetNode<Label>("Margin/Content/Header/HeaderText/NameLabel");
		_subLabel = _panel.GetNode<Label>("Margin/Content/Header/HeaderText/SubLabel");
		_badgeLabel = _panel.GetNode<Label>("Margin/Content/Header/Badge/BadgeLabel");
		_healthBar = _panel.GetNode<ProgressBar>("Margin/Content/HealthRow/HealthBar");
		_healthValueLabel = _panel.GetNode<Label>("Margin/Content/HealthRow/HealthValueLabel");
		_statsGrid = _panel.GetNode<GridContainer>("Margin/Content/StatsGrid");
		_bottomArea = _panel.GetNode<Control>("Margin/Content/BottomArea");
		_skillsRow = _bottomArea.GetNode<HBoxContainer>("SkillsRow");
		_progressBox = _bottomArea.GetNode<VBoxContainer>("ProgressBox");
		_progressLabel = _progressBox.GetNode<Label>("ProgressLabel");
		_progressBar = _progressBox.GetNode<ProgressBar>("ProgressBar");
	}

	public void Process()
	{
		UnitLogic unit = _selection.SelectedUnit;
		Building building = _selection.SelectedBuilding;
		if (unit != null && unit.IsAlive)
		{
			ShowUnit(unit);
			return;
		}
		if (building != null && !building.IsDestroyed)
		{
			ShowBuilding(building);
			return;
		}
		Hide();
	}

	private void ShowUnit(UnitLogic unit)
	{
		IUnitState state = unit.Handle;
		bool hasShield = state.ShieldHealth > 0;
		if (_currentUnit != unit || _shieldEntryVisible != hasShield)
			BuildUnit(unit);
		UpdateUnitHeader(unit, state);
		UpdateHealth(state.Health, state.MaxHealth);
		UpdateUnitLiveStats(state);
		RefreshSkillCooldowns(state);
		_panel.Visible = true;
	}

	private void ShowBuilding(Building building)
	{
		if (_currentBuilding != building)
			BuildBuilding(building);
		UpdateBuildingHeader(building);
		UpdateHealth(building.Health, building.MaxHealth);
		UpdateBuildingLiveStats(building);
		UpdateBuildingProgress(building);
		_panel.Visible = true;
	}

	private void Hide()
	{
		_panel.Visible = false;
		_currentUnit = null;
		_currentBuilding = null;
		ClearSkills();
	}

	private void BuildUnit(UnitLogic unit)
	{
		IUnitState state = unit.Handle;
		_currentUnit = unit;
		_currentBuilding = null;
		_shieldEntryVisible = state.ShieldHealth > 0;
		_portraitRect.Texture = LoadUnitPortrait(state.TypeId) ?? LoadPlaceholder();
		_nameLabel.Text = UnitDisplayName.Get(state.TypeId);
		_badgeLabel.Text = $"费用 {state.UnitCost}";
		BuildUnitStats(state);
		BuildSkills(state);
	}

	private void BuildUnitStats(IUnitState state)
	{
		ClearStats();
		_attackLabel = AddStat(StatIconView.StatIconKind.Attack, BuildAttackText(state), BuildAttackTooltip(state), GetAttackElement(state));
		_armorLabel = AddStat(StatIconView.StatIconKind.Armor, BuildArmorText(state), BuildArmorTooltip(state));
		bool healer = state.Damage <= 0 && state.HealAmount > 0f;
		AddStat(StatIconView.StatIconKind.Cooldown, $"{(healer ? state.HealInterval : state.AttackCooldown):0.##} 秒", healer ? "治疗间隔" : "攻击间隔");
		AddStat(StatIconView.StatIconKind.Range, $"{(healer ? state.HealRange : state.AttackRange):0}", healer ? "治疗范围" : "攻击射程");
		AddStat(StatIconView.StatIconKind.Speed, $"{state.Speed:0}", "移动速度");
		AddStat(StatIconView.StatIconKind.Vision, $"{state.VisionRange:0}", BuildVisionTooltip(state));
		if (_shieldEntryVisible)
			_shieldLabel = AddStat(StatIconView.StatIconKind.Shield, state.ShieldHealth.ToString(), "当前护盾值");
	}

	private void UpdateUnitHeader(UnitLogic unit, IUnitState state)
	{
		string side = unit.IsPlayerUnit ? "我方" : "敌方";
		string status = state.IsStealthed ? " · 隐身中" : "";
		status += state.IsAmbushing ? " · 伏击中" : "";
		status += state.IsFurious ? " · 狂怒" : "";
		_subLabel.Text = side + " · " + RaceName(state.Race) + status;
	}

	private void UpdateUnitLiveStats(IUnitState state)
	{
		_attackLabel.Text = BuildAttackText(state);
		_attackLabel.TooltipText = BuildAttackTooltip(state);
		_armorLabel.Text = BuildArmorText(state);
		_armorLabel.TooltipText = BuildArmorTooltip(state);
		if (_shieldEntryVisible)
			_shieldLabel.Text = state.ShieldHealth.ToString();
	}

	private void BuildSkills(IUnitState state)
	{
		ClearSkills();
		foreach (SkillDef skill in state.Skills.All)
		{
			SkillIconView icon = new();
			icon.Configure(skill);
			_skillsRow.AddChild(icon);
			_skillIcons.Add(icon);
		}
		_skillsRow.Visible = true;
		_progressBox.Visible = false;
		_bottomArea.Visible = state.Skills.All.Count > 0;
	}

	private void RefreshSkillCooldowns(IUnitState state)
	{
		for (int i = 0; i < _skillIcons.Count; i++)
		{
			SkillDef skill = state.Skills.All[i];
			SpecialConfig special = state.Skills.GetSpecialById(skill.Id);
			float total = special == null ? 0f : special.Cooldown;
			_skillIcons[i].SetCooldown(state.GetSkillCooldownRemaining(skill.Id), total);
		}
	}

	private void BuildBuilding(Building building)
	{
		BuildingSystem system = AdapterRegistry.Resolve<BuildingSystem>();
		_currentBuilding = building;
		_currentUnit = null;
		Sprite2D sprite = building.GetNodeOrNull<Sprite2D>("View/Sprite");
		_portraitRect.Texture = sprite == null ? LoadPlaceholder() : sprite.Texture;
		_nameLabel.Text = building.DisplayName;
		_badgeLabel.Text = $"价值 {system.GetValue(building.TypeId)}";
		BuildBuildingStats(building, system);
		ClearSkills();
	}

	private void BuildBuildingStats(Building building, BuildingSystem system)
	{
		ClearStats();
		string type = system.IsCoreBuilding(building.TypeId) ? "核心建筑" : "普通建筑";
		AddStat(StatIconView.StatIconKind.Building, type, "建筑类型");
		AddStat(StatIconView.StatIconKind.Footprint, $"{system.GetFootprint(building.TypeId).Count} 格", "占地面积");
		string unitTypeId = GameManager.Get<IBuildingRepository>().GetUnitTypeId(building.TypeId);
		if (string.IsNullOrEmpty(unitTypeId))
		{
			AddStat(StatIconView.StatIconKind.Production, "不产出单位", "该建筑没有单位生产周期");
			return;
		}
		UnitStats stats = GameManager.Get<IUnitRepository>().Get(unitTypeId);
		AddStat(StatIconView.StatIconKind.Unit, $"{UnitDisplayName.Get(unitTypeId)} · {stats.UnitCost}", "产出单位 · 单次生产成本");
		_productionLabel = AddStat(StatIconView.StatIconKind.Production, BuildProductionText(building, system), BuildProductionTooltip(building, system));
	}

	private void UpdateBuildingHeader(Building building)
	{
		string side = building.IsPlayerOwned ? "我方" : "敌方";
		_subLabel.Text = side + " · " + GetBuildingStatus(building);
	}

	private void UpdateBuildingLiveStats(Building building)
	{
		if (_productionLabel == null)
			return;
		BuildingSystem system = AdapterRegistry.Resolve<BuildingSystem>();
		_productionLabel.Text = BuildProductionText(building, system);
		_productionLabel.TooltipText = BuildProductionTooltip(building, system);
	}

	private void UpdateBuildingProgress(Building building)
	{
		_skillsRow.Visible = false;
		if (building.IsRitualInProgress)
		{
			ShowRitualProgress(building);
			return;
		}
		if (building.HasProductionCycle)
		{
			ShowProductionProgress(building);
			return;
		}
		_bottomArea.Visible = false;
	}

	private void ShowRitualProgress(Building building)
	{
		RitualDef ritual = RitualRules.Get(building.ActiveRitualId);
		float elapsed = building.RitualTotalSeconds - building.RitualRemainingSeconds;
		_progressLabel.Text = $"{ritual.DisplayName} · 剩余 {building.RitualRemainingSeconds:0.0} 秒";
		_progressBar.MaxValue = building.RitualTotalSeconds;
		_progressBar.Value = elapsed;
		_progressBox.Visible = true;
		_bottomArea.Visible = true;
	}

	private void ShowProductionProgress(Building building)
	{
		string state = building.CanWork ? $"剩余 {building.ProductionSecondsRemaining:0.0} 秒" : GetBuildingStatus(building);
		_progressLabel.Text = "生产进度 · " + state;
		_progressBar.MaxValue = 1f;
		_progressBar.Value = building.ProductionProgress;
		_progressBox.Visible = true;
		_bottomArea.Visible = true;
	}

	private Label AddStat(StatIconView.StatIconKind kind, string text, string tooltip, string elementId = "")
	{
		HBoxContainer row = new();
		row.CustomMinimumSize = new Vector2(StatColumnWidth, StatRowHeight);
		row.MouseFilter = Control.MouseFilterEnum.Stop;
		row.TooltipText = tooltip;
		StatIconView icon = new();
		icon.Configure(kind);
		icon.TooltipText = tooltip;
		row.AddChild(icon);
		Label label = CreateStatLabel(text, tooltip);
		row.AddChild(label);
		if (!string.IsNullOrEmpty(elementId))
			row.AddChild(CreateElementIcon(elementId));
		_statsGrid.AddChild(row);
		return label;
	}

	private static Label CreateStatLabel(string text, string tooltip)
	{
		Label label = new();
		label.Text = text;
		label.TooltipText = tooltip;
		label.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		label.AddThemeFontSizeOverride("font_size", 14);
		label.AddThemeColorOverride("font_color", new Color(0.84f, 0.87f, 0.93f));
		label.TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis;
		return label;
	}

	private static ElementIconView CreateElementIcon(string elementId)
	{
		ElementIconView icon = new();
		icon.Configure(elementId);
		return icon;
	}

	private void UpdateHealth(int health, int maxHealth)
	{
		_healthBar.MaxValue = maxHealth;
		_healthBar.Value = health;
		_healthValueLabel.Text = $"{health} / {maxHealth}";
	}

	private void ClearStats()
	{
		foreach (Node child in _statsGrid.GetChildren())
		{
			_statsGrid.RemoveChild(child);
			child.QueueFree();
		}
		_attackLabel = null;
		_armorLabel = null;
		_shieldLabel = null;
		_productionLabel = null;
	}

	private void ClearSkills()
	{
		foreach (Node child in _skillsRow.GetChildren())
		{
			_skillsRow.RemoveChild(child);
			child.QueueFree();
		}
		_skillIcons.Clear();
	}

	private static string BuildAttackText(IUnitState state)
	{
		if (state.Damage <= 0 && state.HealAmount > 0f)
			return $"治疗 {state.HealAmount:0}";
		return $"{state.Damage} · {AttackName(state.AttackType)}·{DamageName(state.DamageType)}";
	}

	private static string BuildAttackTooltip(IUnitState state)
	{
		if (state.Damage <= 0 && state.HealAmount > 0f)
			return $"治疗量：{state.HealAmount:0}\n治疗范围：{state.HealRange:0}\n治疗间隔：{state.HealInterval:0.##} 秒";
		List<string> lines = new() { $"攻击值：{state.Damage}" };
		DamageMatrix matrix = GameManager.Get<DamageMatrix>();
		foreach (ArmorType armor in Enum.GetValues<ArmorType>())
		{
			float multiplier = matrix.GetMultiplier(state.DamageType, armor);
			if (!Mathf.IsEqualApprox(multiplier, NeutralMultiplier))
				lines.Add($"对{ArmorName(armor)} {FormatMultiplier(multiplier)}");
		}
		return string.Join("\n", lines);
	}

	private static string BuildArmorText(IUnitState state)
	{
		return $"{state.EffectiveArmorValue} · {ArmorName(state.ArmorType)}";
	}

	private static string BuildArmorTooltip(IUnitState state)
	{
		int reduction = (int)Math.Round(DamageMatrix.CalculateReduction(state.EffectiveArmorValue) * DamageMatrix.PercentScale);
		List<string> lines = new() { $"护甲减伤：{reduction}%" };
		DamageMatrix matrix = GameManager.Get<DamageMatrix>();
		foreach (DamageType damage in Enum.GetValues<DamageType>())
		{
			float multiplier = matrix.GetMultiplier(damage, state.ArmorType);
			if (!Mathf.IsEqualApprox(multiplier, NeutralMultiplier))
				lines.Add($"受到{DamageName(damage)} {FormatMultiplier(multiplier)}");
		}
		return lines.Count == 1 && reduction == 0 ? NoBonusTooltip : string.Join("\n", lines);
	}

	private static string BuildVisionTooltip(IUnitState state)
	{
		return state.CanDetectStealth ? "视野范围 · 可侦测隐身" : "视野范围";
	}

	private static string BuildProductionText(Building building, BuildingSystem system)
	{
		float rate = building.EffectiveProductionRate;
		if (rate <= 0f)
			rate = system.GetProductionRate(building.TypeId);
		float interval = system.GetSpawnInterval(building.TypeId);
		return $"{rate:0.##}/秒 · {interval:0.##}秒";
	}
	private static string BuildProductionTooltip(Building building, BuildingSystem system)
	{
		float baseRate = system.GetProductionRate(building.TypeId);
		float effectiveRate = building.EffectiveProductionRate > 0f ? building.EffectiveProductionRate : baseRate;
		int bonus = baseRate > 0f ? Mathf.RoundToInt((effectiveRate / baseRate - 1f) * 100f) : 0;
		return $"基础生产率：{baseRate:0.##}/秒\n有效生产率：{effectiveRate:0.##}/秒\n运行时修正：{(bonus >= 0 ? "+" : "")}{bonus}%";
	}

	private static string GetAttackElement(IUnitState state)
	{
		return state.DamageType == DamageType.Holy ? HolyElementId : "";
	}

	private static string FormatMultiplier(float multiplier)
	{
		int percent = Mathf.RoundToInt((multiplier - NeutralMultiplier) * DamageMatrix.PercentScale);
		return percent > 0 ? $"+{percent}%" : $"{percent}%";
	}

	private static string GetBuildingStatus(Building building)
	{
		if (building.IsDestroyed)
			return "已摧毁";
		if (building.IsRitualInProgress)
			return "仪式中";
		if (building.IsManuallyPaused)
			return "手动暂停";
		if (building.IsCombineProhibited)
			return "已禁止组合";
		return building.CanWork ? "工作中" : "休眠";
	}

	public static Texture2D LoadUnitPortrait(string typeId)
	{
		IVisualAssetRepository repository = GameManager.Get<IVisualAssetRepository>();
		string portraitUid = repository.GetPortraitUid(typeId);
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
}