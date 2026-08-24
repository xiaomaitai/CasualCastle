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

	private GameManager _gameManager;
	private KingExperienceService _xpService;
	private ComfortService _comfortService;
	private KingSkillSet _skillSet;
	private UnitLogic _kingLogic;
	private bool _spawned;
	private DungeonSkillPickUiController _skillPick;
	private DungeonKingHudUiController _kingHud;
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
		_skillPick?.HidePanel();
	}

	private DungeonSkillPickUiController SkillPick => _skillPick ??= AdapterRegistry.Resolve<DungeonSkillPickUiController>();

	private DungeonKingHudUiController KingHud => _kingHud ??= AdapterRegistry.Resolve<DungeonKingHudUiController>();

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
			KingHud.SetSkillCount(_skillSet.Count);
		}

		UpdateKingState((float)delta);
		UpdateFactionBuffs();
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
		SkillPick.ShowChoices(level, _pendingChoices, OnSkillChosen);
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
		SkillPick.HidePanel();
		KingHud.SetSkillCount(_skillSet.Count);
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
}