// 魔王控制器：生成魔王、驱动舒适度/经验与升级技能三选一
using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Battle;
using CasualCastle.Domain.Building;
using Godot;
using System.Collections.Generic;

public partial class DemonKingController : Node
{
	private const string UnitPrefabPath = "res://prefabs/Unit.tscn";
	private const float KingSpawnOffsetY = 70f;
	private const float PanelWidth = 760f;
	private const float PanelHeight = 460f;

	private GameManager _gameManager;
	private KingExperienceService _xpService;
	private ComfortService _comfortService;
	private KingSkillSet _skillSet;
	private UnitLogic _kingLogic;
	private bool _spawned;
	private Control _skillPanel;
	private ProgressBar _comfortBar;
	private Label _statusLabel;
	private List<string> _pendingChoices = new();

	public override void _Ready()
	{
		_gameManager = AdapterRegistry.Resolve<GameManager>();
		_xpService = GameManager.Get<KingExperienceService>();
		_comfortService = GameManager.Get<ComfortService>();
		_skillSet = GameManager.Get<KingSkillSet>();
		if (_xpService != null)
			_xpService.LevelUp += OnKingLevelUp;
	}

	public override void _ExitTree()
	{
		if (_xpService != null)
			_xpService.LevelUp -= OnKingLevelUp;
		HideSkillPanel();
	}

	public override void _Process(double delta)
	{
		if (_gameManager == null || _gameManager.SelectedFaction != FactionDef.DungeonId)
			return;

		if (_gameManager.CurrentState != GameManager.GameState.Playing)
			return;

		if (!_spawned)
		{
			TrySpawnKing();
			if (!_spawned)
				return;
		}

		UpdateKingState((float)delta);
		UpdateFactionBuffs();
		UpdateHud();
	}

	private void TrySpawnKing()
	{
		Castle playerCastle = _gameManager.PlayerCastle;
		if (playerCastle?.Heart == null || _gameManager.Battlefield == null)
			return;

		PackedScene unitScene = GD.Load<PackedScene>(UnitPrefabPath);
		if (unitScene == null)
			return;

		Unit shell = unitScene.Instantiate<Unit>();
		UnitLogic logic = shell.GetNode<UnitLogic>("Logic");
		logic.IsPlayerUnit = true;
		logic.InitializeFromStats(GameManager.Get<IUnitRepository>().Get("DemonKing"));
		logic.SetComfort(_comfortService);

		Vector2 heartGlobal = playerCastle.Heart.GlobalPosition;
		float centerX = GameCoordinatesAdapter.PixelsToGameUnits(heartGlobal.X);
		float centerY = GameCoordinatesAdapter.PixelsToGameUnits(heartGlobal.Y);
		float patrolRadius = DemonKingRules.GetPatrolRadius(_xpService.Level);
		logic.ConfigureDefender(centerX, centerY, patrolRadius);

		shell.GlobalPosition = heartGlobal + new Vector2(0f, KingSpawnOffsetY);
		_gameManager.Battlefield.AddChild(shell);

		_kingLogic = logic;
		_spawned = true;
		ApplyLearnedSkills();
		CreateHud(shell);
		GD.Print($"魔王已生成 Lv{_xpService.Level}");
	}

	private void ApplyLearnedSkills()
	{
		if (_kingLogic == null)
			return;
		ISkillRepository skillRepo = GameManager.Get<ISkillRepository>();
		foreach (string skillId in _skillSet.GetAll())
		{
			if (skillRepo == null)
				continue;
			SkillDef scaled = DemonKingSkillScaling.Scale(skillRepo.Get(skillId), _skillSet.GetLearnLevel(skillId), _xpService.Level);
			_kingLogic.ApplyKingSkill(scaled);
		}
	}

	private void UpdateKingState(float delta)
	{
		Building heart = _gameManager.PlayerCastle?.Heart;
		if (heart == null)
			return;
		Vector2 thronePixels = heart.GlobalPosition;
		float throneX = GameCoordinatesAdapter.PixelsToGameUnits(thronePixels.X);
		float throneY = GameCoordinatesAdapter.PixelsToGameUnits(thronePixels.Y);
		float dx = _kingLogic.Handle.GameX - throneX;
		float dy = _kingLogic.Handle.GameY - throneY;
		float distance = Mathf.Sqrt(dx * dx + dy * dy);
		bool nearThrone = distance <= DemonKingRules.ThroneRestoreRadius;
		_comfortService.TickRestore(delta, nearThrone, _gameManager.IsCombat);
		_comfortService.TickDistancePenalty(delta, distance, _kingLogic.Handle.PatrolRadius);
		_kingLogic.SetResting(!_comfortService.IsCombatReady);
	}

	private void UpdateFactionBuffs()
	{
		Castle castle = _gameManager.PlayerCastle;
		if (castle == null)
			return;
		List<IBuildingState> states = castle.GetBuildingStates();
		bool bloodThrone = HasBuilding(states, "KhorneBloodThrone");
		bool labyrinth = HasBuilding(states, "TzeentchLabyrinth");
		bool paradise = HasBuilding(states, "SlaaneshParadise");
		bool garden = HasBuilding(states, "NurgleGarden");
		bool avatarSkill = _skillSet.Contains("khorne_avatar");
		bool paradiseSkill = _skillSet.Contains("paradise_realm");
		bool gardenSkill = _skillSet.Contains("nurgle_garden");
		bool labyrinthSkill = _skillSet.Contains("labyrinth");

		DungeonFactionBuffs.ActivePatrolEnabled = bloodThrone;
		DungeonFactionBuffs.TrapCooldownMultiplier = labyrinth || labyrinthSkill
			? DungeonFactionBuffs.TrapCooldownHalvedMultiplier
			: 1f;
		DungeonFactionBuffs.ComfortMaxBonus = (paradise ? DungeonFactionBuffs.ComfortParadiseBonus : 0f)
			+ (paradiseSkill ? DungeonFactionBuffs.ComfortParadiseBonus : 0f);
		DungeonFactionBuffs.KillXpMultiplier = bloodThrone
			? DungeonFactionBuffs.KillXpBloodThroneMultiplier
			: 1f;
		DungeonFactionBuffs.GlobalRegenPerSecond = garden || gardenSkill
			? DungeonFactionBuffs.GlobalRegenFractionPerSecond
			: 0f;
		DungeonFactionBuffs.AutoBreedEnabled = garden || gardenSkill;

		SlaaneshComfortBuffs slaanesh = SlaaneshServiceRules.CalculateComfortBuffs(states, GameManager.Get<IBuildingRepository>(), paradise || paradiseSkill);
		_comfortService.RestoreMultiplier = slaanesh.RestoreMultiplier;
		_comfortService.AttackCostMultiplier = slaanesh.AttackCostMultiplier;
		_comfortService.HitCostMultiplier = slaanesh.HitCostMultiplier;
		float globalCost = avatarSkill ? 2f : 1f;
		if (slaanesh.HalfCostBelowHalfComfort && _comfortService.Value < _comfortService.MaxComfort * 0.5f)
			globalCost *= 0.5f;
		_comfortService.GlobalCostMultiplier = globalCost;
		_comfortService.SetMaxComfort(DemonKingRules.MaxComfort + DungeonFactionBuffs.ComfortMaxBonus + slaanesh.MaxComfortBonus);
		DungeonFactionBuffs.ProductionSpeedMultiplier = _skillSet.Contains("fast_breed")
			? DungeonFactionBuffs.FastBreedProductionMultiplier
			: 1f;
		DungeonFactionBuffs.PlagueSpreadPerSecond = _skillSet.Contains("plague_spread")
			? DungeonFactionBuffs.PlagueSpreadPerSecondValue
			: 0f;
		DungeonFactionBuffs.ManaProductionMultiplier = labyrinth || labyrinthSkill
			? DungeonFactionBuffs.ManaProductionLabyrinthMultiplier
			: 1f;
		DungeonFactionBuffs.TrapCapDoubled = labyrinth || labyrinthSkill;
		DungeonFactionBuffs.AuraRangeMultiplier = _skillSet.Contains("seductive_voice")
			? DungeonFactionBuffs.AuraRangeSkillMultiplier
			: 1f;
		DungeonFactionBuffs.AttackDamageMultiplier = HasBuilding(states, "PleasureStage")
			? DungeonFactionBuffs.PleasureStageAttackMultiplier
			: 1f;
		_xpService.KillXpMultiplier = DungeonFactionBuffs.KillXpMultiplier;
		float patrolRadius = DungeonFactionBuffs.ActivePatrolEnabled
			? DungeonFactionBuffs.ActivePatrolRadius
			: DemonKingRules.GetPatrolRadius(_xpService.Level);
		_kingLogic.SetPatrolRadius(patrolRadius);
	}

	private static bool HasBuilding(List<IBuildingState> states, string typeId)
	{
		foreach (IBuildingState state in states)
		{
			if (state.TypeId == typeId && !state.IsDestroyed)
				return true;
		}
		return false;
	}
	private void OnKingLevelUp(int level)
	{
		ReapplyKingSkills();
		_pendingChoices = DemonKingSkillPool.GenerateChoices(System.Random.Shared, level, _skillSet.GetAll());
		UpgradeThroneToLevel(level);
		ShowSkillPanel(level);
	}

	private void ReapplyKingSkills()
	{
		if (_kingLogic == null)
			return;
		ISkillRepository skillRepo = GameManager.Get<ISkillRepository>();
		foreach (string skillId in _skillSet.GetAll())
		{
			_kingLogic.RemoveKingSkill(skillId);
			SkillDef baseDef = skillRepo == null ? null : skillRepo.Get(skillId);
			if (baseDef == null)
				continue;
			SkillDef scaled = DemonKingSkillScaling.Scale(baseDef, _skillSet.GetLearnLevel(skillId), _xpService.Level);
			_kingLogic.ApplyKingSkill(scaled);
		}
	}

	private void UpgradeThroneToLevel(int level)
	{
		Castle castle = _gameManager.PlayerCastle;
		if (castle?.Heart == null || level < 1 || level > DungeonCoreRules.CoreLevelTypeIds.Length)
			return;
		string typeId = DungeonCoreRules.CoreLevelTypeIds[level - 1];
		if (castle.Heart.TypeId == typeId)
			return;
		castle.ReplaceHeart(typeId, castle.Heart.Health);
	}

	private void ShowSkillPanel(int level)
	{
		if (_skillPanel != null)
			return;

		CanvasLayer layer = new CanvasLayer { Layer = 100 };
		AddChild(layer);
		Panel panel = new Panel { Size = new Vector2(PanelWidth, PanelHeight) };
		panel.Position = new Vector2((1920f - PanelWidth) * 0.5f, (1080f - PanelHeight) * 0.5f);
		layer.AddChild(panel);

		Label title = new Label { Text = $"魔王升级至 Lv{level}，选择一项技能" };
		title.Position = new Vector2(40f, 28f);
		title.Size = new Vector2(PanelWidth - 80f, 60f);
		title.AddThemeFontSizeOverride("font_size", 34);
		panel.AddChild(title);

		VBoxContainer list = new VBoxContainer();
		list.Position = new Vector2(40f, 108f);
		list.Size = new Vector2(PanelWidth - 80f, PanelHeight - 140f);
		list.AddThemeConstantOverride("separation", 14);
		panel.AddChild(list);

		ISkillRepository skillRepo = GameManager.Get<ISkillRepository>();
		foreach (string skillId in _pendingChoices)
		{
			SkillDef def = skillRepo == null ? null : skillRepo.Get(skillId);
			if (def == null)
				continue;
			string godName = GetGodName(DemonKingSkillPool.GetGod(skillId));
			string ultimateMark = DemonKingSkillPool.IsUltimate(skillId) ? "【终极】" : "";
			Button button = new Button
			{
				Text = $"{ultimateMark}{def.DisplayName}（{godName}）{GetSkillDescription(def)}"
			};
			button.CustomMinimumSize = new Vector2(0f, 70f);
			button.AddThemeFontSizeOverride("font_size", 26);
			button.Pressed += () => OnSkillChosen(skillId);
			list.AddChild(button);
		}
		_skillPanel = panel;
	}

	private static string GetSkillDescription(SkillDef def)
	{
		return string.IsNullOrEmpty(def.Description) ? "" : $"\n{def.Description}";
	}

	private void OnSkillChosen(string skillId)
	{
		_skillSet.Add(skillId, _xpService.Level);
		ISkillRepository skillRepo = GameManager.Get<ISkillRepository>();
		if (skillRepo != null && _kingLogic != null)
		{
			_kingLogic.ApplyKingSkill(skillRepo.Get(skillId));
			if (skillId == "khorne_avatar")
				_kingLogic.ApplyKingSkill(skillRepo.Get("khorne_avatar_cleave"));
		}
		if (SummonedBuildings.TryGetValue(skillId, out string buildingType))
			TryPlaceBuildingNearThrone(buildingType);
		Sfx.Play(SfxEvents.Upgrade);
		HideSkillPanel();
	}

	private static readonly Dictionary<string, string> SummonedBuildings = new()
	{
		{ "spike_trap_skill", "SpikeTrap" },
		{ "poison_trap_skill", "PoisonTrap" },
		{ "fire_trap_skill", "FireTrap" },
		{ "teleport_trap", "TeleportTrap" },
		{ "grand_hall", "HotSpring" },
		{ "feast", "BanquetHall" },
		{ "charm_servitude", "HeartPortal" },
	};

	private void TryPlaceBuildingNearThrone(string buildingType)
	{
		Castle castle = _gameManager.PlayerCastle;
		if (castle?.Heart == null)
			return;
		int throneX = castle.Heart.AnchorGridX;
		int throneY = castle.Heart.AnchorGridY;
		BuildingSystem system = AdapterRegistry.Resolve<BuildingSystem>();
		(int, int)[] offsets = { (-1, 0), (1, 0), (0, -1), (0, 1), (2, 0), (-2, 0), (0, 2), (0, -2) };
		foreach ((int dx, int dy) in offsets)
		{
			if (system.TryPlaceForCastle(castle, buildingType, throneX + dx, throneY + dy))
				return;
		}
	}

	private void TickPassiveMana(float delta)
	{
		if (!_skillSet.Contains("tzeentch_rune"))
			return;
		GameManager.Get<ManaService>()?.Add(2f * delta);
	}

	private void HideSkillPanel()
	{
		if (_skillPanel == null)
			return;
		Node parent = _skillPanel.GetParent();
		parent?.RemoveChild(_skillPanel);
		_skillPanel.QueueFree();
		_skillPanel = null;
		_pendingChoices.Clear();
	}

	private void CreateHud(Unit shell)
	{
		Label label = new Label { Text = "Lv1" };
		label.Position = new Vector2(-80f, -150f);
		label.Size = new Vector2(180f, 30f);
		label.HorizontalAlignment = HorizontalAlignment.Center;
		label.AddThemeFontSizeOverride("font_size", 22);
		shell.AddChild(label);

		ProgressBar bar = new ProgressBar { MaxValue = DemonKingRules.MaxComfort };
		bar.Position = new Vector2(-70f, -122f);
		bar.Size = new Vector2(140f, 14f);
		shell.AddChild(bar);
		_comfortBar = bar;
		_statusLabel = label;
	}

	private void UpdateHud()
	{
		if (_comfortBar != null)
			_comfortBar.Value = _comfortService.Value;
		if (_statusLabel == null)
			return;
		string skillText = _skillSet.Count > 0 ? $" · 技能{_skillSet.Count}" : "";
		_statusLabel.Text = $"Lv{_xpService.Level} · 舒适{_comfortService.Value:F0}{skillText}";
	}

	private static string GetGodName(string godId)
	{
		switch (godId)
		{
			case DemonKingSkillPool.Khorne: return "恐虐";
			case DemonKingSkillPool.Tzeentch: return "奸奇";
			case DemonKingSkillPool.Slaanesh: return "色孽";
			case DemonKingSkillPool.Nurgle: return "纳垢";
			default: return "未知";
		}
	}
}