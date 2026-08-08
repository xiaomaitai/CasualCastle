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
	private readonly Button _upgradeButton;
	private readonly UnitSelectionController _selection;
	private readonly List<SkillIconView> _skillIcons = new();
	private readonly TextureRect _burrowIconRect;
	private readonly Label _burrowProgressLabel;
	private readonly VBoxContainer _ritualBox;
	private readonly Label _ritualStatusLabel;
	private readonly List<RitualButtonEntry> _ritualButtons = new();
	private bool _ritualShowingProgress;

	private sealed class RitualButtonEntry
	{
		public Button Button;
		public RitualDef Def;
	}
	private UnitLogic _currentUnit;
	private Building _currentBuilding;
	private UpgradeRecipe _pendingUpgrade;

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
		_upgradeButton = uiRoot.GetNode<Button>("AttributePanel/UpgradeButton");
		_upgradeButton.Visible = false;
		_upgradeButton.Pressed += OnUpgradePressed;
		_panel.Visible = false;

		_burrowIconRect = new TextureRect
		{
			Texture = BuildingIcons.Burrow,
			Position = new Vector2(16, 192),
			CustomMinimumSize = new Vector2(30, 30),
			ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
			StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
			MouseFilter = Control.MouseFilterEnum.Ignore,
		};
		_burrowIconRect.Visible = false;
		_panel.AddChild(_burrowIconRect);

		_burrowProgressLabel = new Label
		{
			Position = new Vector2(52, 194),
			MouseFilter = Control.MouseFilterEnum.Ignore,
		};
		_burrowProgressLabel.AddThemeFontSizeOverride("font_size", 18);
		_burrowProgressLabel.Visible = false;
		_panel.AddChild(_burrowProgressLabel);

		_ritualBox = new VBoxContainer
		{
			Position = new Vector2(16, 244),
			CustomMinimumSize = new Vector2(424, 56),
			MouseFilter = Control.MouseFilterEnum.Pass,
		};
		_panel.AddChild(_ritualBox);

		_ritualStatusLabel = new Label
		{
			MouseFilter = Control.MouseFilterEnum.Ignore,
			Text = "举行仪式：",
		};
		_ritualStatusLabel.AddThemeFontSizeOverride("font_size", 17);
		_ritualBox.AddChild(_ritualStatusLabel);
		_ritualBox.Visible = false;
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
			else
				UpdateRitualStatus(building);
			_panel.Visible = true;
			return;
		}

		_panel.Visible = false;
		_currentUnit = null;
		_currentBuilding = null;
		_pendingUpgrade = null;
		_upgradeButton.Visible = false;
		HideBurrowProgress();
		HideRitualPanel();
		ClearSkillsRow();
	}

	private void RebuildUnitPanel(UnitLogic unit)
	{
		_currentUnit = unit;
		_currentBuilding = null;
		HideBurrowProgress();
		HideRitualPanel();
		IUnitState state = unit.Handle;

		_portraitRect.Texture = LoadUnitPortrait(state.TypeId) ?? LoadPlaceholder();
		_nameLabel.Text = UnitDisplayName.Get(state.TypeId);
		_subLabel.Text = (unit.IsPlayerUnit ? "我方" : "敌方")
			+ " · " + RaceName(state.Race)
			+ (state.IsStealthed ? " · 隐身中" : "")
			+ (state.IsAmbushing ? " · 伏击中" : "");
		_statsLabel.Text = BuildUnitStats(state) + BuildBonusLine(unit);

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
		RefreshUpgradeButton(building);
		RefreshRitualPanel(building);
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
			+ $"护甲 {ArmorName(state.ArmorType)} {state.EffectiveArmorValue}   费用 {state.UnitCost}   状态 {status}\n"
			+ shieldLine + healLine;
	}

	private static string BuildBuildingStats(Building building)
	{
		BuildingSystem system = AdapterRegistry.Resolve<BuildingSystem>();
		string type = system.IsCoreBuilding(building.TypeId) ? "核心建筑" : "普通建筑";
		string lines = $"生命 {building.Health}/{building.MaxHealth}   占地 {system.GetFootprint(building.TypeId).Count} 格\n"
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

	private static string BuildBonusLine(UnitLogic unit)
	{
		string line = "";
		if (unit.IsFurious)
		{
			line += "狂怒中\n";
			if (unit.FuryDamageMultiplier > 1.01f)
				line += $"狂怒 攻击 +{Mathf.RoundToInt((unit.FuryDamageMultiplier - 1f) * 100f)}%\n";
			if (unit.FuryAttackSpeedMultiplier > 1.01f)
				line += $"狂怒 攻速 +{Mathf.RoundToInt((unit.FuryAttackSpeedMultiplier - 1f) * 100f)}%\n";
			if (unit.FuryMoveSpeedMultiplier > 1.01f)
				line += $"狂怒 移速 +{Mathf.RoundToInt((unit.FuryMoveSpeedMultiplier - 1f) * 100f)}%\n";
		}
		if (unit.AuraAttackSpeedMultiplier > 1.01f)
			line += $"攻速光环 +{Mathf.RoundToInt((unit.AuraAttackSpeedMultiplier - 1f) * 100f)}%\n";
		if (unit.AuraMoveSpeedMultiplier > 1.01f)
			line += $"移速光环 +{Mathf.RoundToInt((unit.AuraMoveSpeedMultiplier - 1f) * 100f)}%\n";
		if (unit.AuraAttackDamageMultiplier > 1.01f)
			line += $"攻击光环 +{Mathf.RoundToInt((unit.AuraAttackDamageMultiplier - 1f) * 100f)}%\n";
		if (unit.AuraRegenPerSecond > 0f)
			line += "回血光环\n";
		if (unit.AuraExecuteDamageMultiplier > 1.01f)
			line += $"斩杀光环 +{Mathf.RoundToInt((unit.AuraExecuteDamageMultiplier - 1f) * 100f)}%\n";
		if (unit.AuraArmorBonus > 0)
			line += $"护甲光环 +{unit.AuraArmorBonus}\n";
		return line;
	}

	private void RefreshUpgradeButton(Building building)
	{
		_pendingUpgrade = null;
		HideBurrowProgress();
		if (!building.IsPlayerOwned)
		{
			_upgradeButton.Visible = false;
			return;
		}

		if (RitualCircleRules.GetLevel(building.TypeId) >= RitualCircleRules.CoreLevelTypeIds.Length)
		{
			_upgradeButton.Visible = false;
			return;
		}

		if (RitualCircleRules.GetLevel(building.TypeId) > 0)
		{
			_upgradeButton.Text = "通过完成仪式升级";
			_upgradeButton.Disabled = true;
			_upgradeButton.Visible = true;
			return;
		}

		if (HordeCoreRules.GetLevel(building.TypeId) >= HordeCoreRules.CoreLevelTypeIds.Length)
		{
			ShowBurrowCount(HordeCoreRules.CountBurrows(building.GetCastle().GetBuildingStates()));
			_upgradeButton.Visible = false;
			return;
		}

		UpgradeRules rules = GameManager.Get<UpgradeRules>();
		UpgradeRecipe recipe = rules.FindRecipe(building.TypeId);
		if (recipe == null)
		{
			_upgradeButton.Visible = false;
			return;
		}

		int cost = rules.CalculateCost(building.TypeId, recipe.ResultTypeId, GameManager.Get<IBuildingRepository>());
		if (cost <= 0)
		{
			_upgradeButton.Visible = false;
			return;
		}

		IBuildingRepository repo = GameManager.Get<IBuildingRepository>();
		List<IBuildingState> states = building.GetCastle().GetBuildingStates();
		bool isNecromancer = repo.GetFactionId(building.TypeId) == FactionDef.NecromancerId;
		int coreLevel = isNecromancer ? RitualCircleRules.GetCoreLevel(states) : HordeCoreRules.GetCoreLevel(states);
		int burrowCount = HordeCoreRules.CountBurrows(states);
		string blockReason = "";
		bool upgradeBlocked = false;
		if (HordeCoreRules.IsCoreUpgrade(building.TypeId, recipe.ResultTypeId))
		{
			int required = HordeCoreRules.GetUpgradeBurrowRequirement(HordeCoreRules.GetLevel(building.TypeId));
			ShowBurrowProgress(burrowCount, required);
			upgradeBlocked = burrowCount < required;
		}
		else if (repo.GetFactionId(building.TypeId) == FactionDef.HordeId)
		{
			int required = HordeCoreRules.GetRequiredCoreLevel(recipe.ResultTypeId, repo);
			if (coreLevel < required)
			{
				blockReason = $"（需核心 {required} 级）";
				upgradeBlocked = true;
			}
		}
		else if (isNecromancer)
		{
			int required = RitualCircleRules.GetRequiredCoreLevel(recipe.ResultTypeId, repo);
			if (coreLevel < required)
			{
				blockReason = $"（需核心 {required} 级）";
				upgradeBlocked = true;
			}
		}

		_pendingUpgrade = recipe;
		string resultName = AdapterRegistry.Resolve<BuildingSystem>().GetDisplayName(recipe.ResultTypeId);
		_upgradeButton.Text = $"升级为{resultName}（{cost} 金币）{blockReason}";
		_upgradeButton.Disabled = upgradeBlocked;
		_upgradeButton.Visible = true;
	}

	private void ShowBurrowProgress(int current, int required)
	{
		_burrowIconRect.Visible = true;
		_burrowProgressLabel.Visible = true;
		_burrowProgressLabel.Text = $"{current}/{required}";
	}

	private void ShowBurrowCount(int current)
	{
		_burrowIconRect.Visible = true;
		_burrowProgressLabel.Visible = true;
		_burrowProgressLabel.Text = $"{current}";
	}

	private void HideBurrowProgress()
	{
		_burrowIconRect.Visible = false;
		_burrowProgressLabel.Visible = false;
	}

	private void RefreshRitualPanel(Building building)
	{
		HideRitualPanel();
		if (building == null || !building.IsPlayerOwned || !RitualRules.CanHostRitual(building.TypeId))
			return;

		_ritualBox.Visible = true;
		if (building.IsRitualInProgress)
		{
			_ritualShowingProgress = true;
			_ritualStatusLabel.Text = "仪式中：" + GetRitualProgressText(building);
			return;
		}

		_ritualStatusLabel.Text = "举行仪式：";
		HBoxContainer row = new HBoxContainer
		{
			MouseFilter = Control.MouseFilterEnum.Ignore,
		};
		_ritualBox.AddChild(row);
		foreach (RitualDef def in RitualRules.GetAll())
		{
			Button button = new Button
			{
				Text = $"{def.DisplayName} {GetRitualCostText(def)}",
				Disabled = !IsRitualAvailable(building, def),
			};
			button.AddThemeFontSizeOverride("font_size", 15);
			string ritualId = def.Id;
			button.Pressed += () => TryStartRitual(building, ritualId);
			row.AddChild(button);
			_ritualButtons.Add(new RitualButtonEntry { Button = button, Def = def });
		}
	}

	private void UpdateRitualStatus(Building building)
	{
		if (!_ritualBox.Visible || building == null)
			return;
		bool inProgress = building.IsRitualInProgress;
		if (inProgress != _ritualShowingProgress)
		{
			RefreshRitualPanel(building);
			return;
		}
		if (inProgress)
		{
			_ritualStatusLabel.Text = "仪式中：" + GetRitualProgressText(building);
			return;
		}
		foreach (RitualButtonEntry entry in _ritualButtons)
			entry.Button.Disabled = !IsRitualAvailable(building, entry.Def);
	}

	private void TryStartRitual(Building building, string ritualId)
	{
		if (building == null || !GodotObject.IsInstanceValid(building))
			return;
		if (!building.TryStartRitual(ritualId))
			return;

		Sfx.Play(SfxEvents.Upgrade);
		RefreshRitualPanel(building);
	}

	private void HideRitualPanel()
	{
		_ritualBox.Visible = false;
		_ritualShowingProgress = false;
		foreach (Node child in _ritualBox.GetChildren())
		{
			if (child == _ritualStatusLabel)
				continue;
			_ritualBox.RemoveChild(child);
			child.QueueFree();
		}
		_ritualButtons.Clear();
	}

	private static string GetRitualProgressText(Building building)
	{
		RitualDef def = RitualRules.Get(building.ActiveRitualId);
		string name = def?.DisplayName ?? building.ActiveRitualId;
		return $"{name} {Mathf.CeilToInt(building.RitualRemainingSeconds)}s / {Mathf.CeilToInt(building.RitualTotalSeconds)}s";
	}

	private static string GetRitualCostText(RitualDef def)
	{
		string cost = def.CostType == RitualCostType.Gold ? $"{def.CostAmount} 金币" : $"{def.CostAmount} 黑魔法";
		return $"{cost}·{def.DurationSeconds:0}s";
	}

	private static bool IsRitualAvailable(Building building, RitualDef def)
	{
		if (building.GetCastle() == null)
			return false;
		int coreLevel = RitualCircleRules.GetCoreLevel(building.GetCastle().GetBuildingStates());
		if (coreLevel < def.RequiredCoreLevel)
			return false;
		if (def.CostType == RitualCostType.Gold)
			return AdapterRegistry.Resolve<Shop>().CanAfford(def.CostAmount);
		BlackMagicService blackMagic = GameManager.Get<BlackMagicService>();
		return blackMagic != null && blackMagic.Value >= def.CostAmount;
	}

	private void OnUpgradePressed()
	{
		if (_pendingUpgrade == null || _currentBuilding == null || !GodotObject.IsInstanceValid(_currentBuilding))
			return;

		Building building = _currentBuilding;
		float healthRatio = building.MaxHealth > 0 ? building.Health / (float)building.MaxHealth : 1f;
		Castle castle = building.GetCastle();
		if (castle == null)
			return;

		IUpgradeUseCase useCase = GameManager.Get<IUpgradeUseCase>();
		UpgradeBuildingFactory factory = new(castle);
		List<IBuildingState> states = building.GetCastle().GetBuildingStates();
		IBuildingState upgraded = useCase.TryUpgrade(
			building, _pendingUpgrade.ResultTypeId, healthRatio,
			AdapterRegistry.Resolve<Shop>().TrySpendGold, factory,
			GameManager.Get<IBuildingRepository>().GetFactionId(building.TypeId) == FactionDef.NecromancerId
				? RitualCircleRules.GetCoreLevel(states)
				: HordeCoreRules.GetCoreLevel(states),
			HordeCoreRules.CountBurrows(states));
		if (upgraded == null)
			return;

		Sfx.Play(SfxEvents.Upgrade);

		_currentBuilding = null;
		_pendingUpgrade = null;
		_upgradeButton.Visible = false;
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

	public static Texture2D LoadUnitPortrait(string typeId)
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
			DamageType.Shadow => "暗影",
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
			"Orc" => "兽人",
			"Goblin" => "哥布林",
			"Centaur" => "半人马",
			"Staglur" => "半人鹿",
			"Lizard" => "蜥蜴人",
			"Undead" => "亡灵",
			_ => race,
		};
	}
}
