// 亡灵派系 v0.4 战斗冒烟：真实对局验证夜战产兵、仪式升级、T2 升级与死亡圣殿组合。
using System.Collections.Generic;
using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Battle;
using CasualCastle.Domain.Building;
using CasualCastle.Domain.Shared;
using Godot;

public partial class NecromancerBattleSmoke : Node
{
	private const string MainGameScene = "res://scenes/main/main_game.tscn";
	private const float NightSpawnWait = 4f;
	private const float CombineCheckDelay = 0.5f;
	private const float RitualTickWait = 1.2f;
	private const float RitualPauseWait = 0.8f;
	private const float TotalTimeout = 60f;
	private const float WorkSpeedBoost = 10f;

	private enum SmokeStep
	{
		CheckSpellBarUi,
		WaitNight,
		WaitSpawns,
		WaitFirstRitual,
		WaitSecondRitual,
		VerifyCombine,
		CheckRitualPhaseGate
	}

	private readonly List<string> _failures = new();
	private Castle _playerCastle;
	private Building _tomb;
	private Building _darkAltar;
	private readonly List<Building> _spellBarTargets = new();
	private bool _spellBarExpectVisible;
	private bool _spellBarExpectSelected;
	private int _spellBarTargetIndex = -1;
	private int _spellBarSettleFrames;
	private int _ritualGateStep;
	private float _ritualRemainingAtStart;
	private float _ritualRemainingAtCombatEnd;
	private Vector2 _inputScale = Vector2.One;
	private bool _inputScaleReady;
	private SmokeStep _step = SmokeStep.CheckSpellBarUi;
	private float _elapsed;
	private float _stepStartedAt;
	private bool _nightObserved;

	public override void _Input(InputEvent @event)
	{
		if (_inputScaleReady || @event is not InputEventMouseButton mouse || !mouse.Pressed)
			return;
		_inputScale = mouse.GlobalPosition / Vector2.One;
		_inputScaleReady = true;
	}

	public override void _Ready()
	{
		AdapterRegistry.Resolve<GameManager>().SetSelectedFaction(FactionDef.NecromancerId);
		AddChild(GD.Load<PackedScene>(MainGameScene).Instantiate());
		_playerCastle = AdapterRegistry.Resolve<GameManager>().PlayerCastle;
		PlaceBuildings();
	}

	public override void _Process(double delta)
	{
		if (_playerCastle == null)
			return;
		_elapsed += (float)delta;
		AdvanceStep();
		if (_elapsed > TotalTimeout)
		{
			_failures.Add("冒烟超时");
			ReportAndQuit();
		}
	}

	private void AdvanceStep()
	{
		switch (_step)
		{
			case SmokeStep.CheckSpellBarUi:
				CheckSpellBarUi();
				break;
			case SmokeStep.WaitNight:
				WaitNight();
				break;
			case SmokeStep.WaitSpawns:
				WaitSpawns();
				break;
			case SmokeStep.WaitFirstRitual:
				WaitFirstRitual();
				break;
			case SmokeStep.WaitSecondRitual:
				WaitSecondRitual();
				break;
			case SmokeStep.VerifyCombine:
				VerifyCombine();
				break;
			case SmokeStep.CheckRitualPhaseGate:
				CheckRitualPhaseGate();
				break;
		}
	}

	private void PlaceBuildings()
	{
		BuildingSystem system = AdapterRegistry.Resolve<BuildingSystem>();
		if (_playerCastle == null)
		{
			_failures.Add("玩家城堡缺失");
			return;
		}
		if (_playerCastle.Heart == null || _playerCastle.Heart.TypeId != "RitualCircleL1")
			_failures.Add($"仪式之环 L1 缺失 type={_playerCastle.Heart?.TypeId}");
		_tomb = TryPlaceBuilding(system, "Tomb", 0, 0);
		TryPlaceBuilding(system, "BlightField", 2, 0);
		_darkAltar = TryPlaceBuilding(system, "DarkAltar", 0, 2);
	}

	private Building TryPlaceBuilding(BuildingSystem system, string typeId, int gridX, int gridY)
	{
		if (!system.TryPlace(_playerCastle, typeId, gridX, gridY))
		{
			_failures.Add($"{typeId} 放置失败");
			return null;
		}
		return FindBuilding(typeId);
	}

	private void CheckSpellBarUi()
	{
		if (_spellBarTargetIndex < 0)
		{
			_spellBarTargets.Add(_darkAltar);
			_spellBarTargets.Add(_playerCastle.Heart);
			_spellBarTargets.Add(_tomb);
			_spellBarTargets.Add(null);
			_spellBarTargetIndex = 0;
			ApplySpellBarTarget();
			return;
		}

		if (_spellBarSettleFrames > 0)
		{
			_spellBarSettleFrames--;
			return;
		}

		VerifySpellBarState();
		_spellBarTargetIndex++;
		if (_spellBarTargetIndex >= _spellBarTargets.Count)
		{
			GD.Print("SMOKE: spell bar ui ok");
			_stepStartedAt = _elapsed;
			_step = SmokeStep.WaitNight;
			return;
		}
		ApplySpellBarTarget();
	}

	private void ApplySpellBarTarget()
	{
		Building target = _spellBarTargets[_spellBarTargetIndex];
		_spellBarExpectVisible = target != null
			&& target.IsPlayerOwned
			&& BuildingActionRules.CanUseSpellBar(target.TypeId);
		_spellBarExpectSelected = target != null;
		_spellBarSettleFrames = 4;
		Vector2 screenPoint = target == null
			? GetEmptyScreenPoint()
			: GetViewport().GetCanvasTransform() * target.GlobalPosition;
		PushBattlefieldClick(screenPoint);
	}

	private void VerifySpellBarState()
	{
		Control handPanel = GetNode<Control>("MainGame/UI/HandPanel");
		Control spellBar = GetNode<Control>("MainGame/UI/SpellBarPanel");
		HBoxContainer slots = GetNode<HBoxContainer>("MainGame/UI/SpellBarPanel/SpellSlots");
		Control attributePanel = GetNode<Control>("MainGame/UI/AttributePanel");
		if (attributePanel.Visible != _spellBarExpectSelected)
			_failures.Add($"选中切换错误 attributeVisible={attributePanel.Visible} 期望={_spellBarExpectSelected}");
		if (handPanel.Visible == _spellBarExpectVisible)
			_failures.Add($"法术栏手牌切换错误 handVisible={handPanel.Visible} 期望={_spellBarExpectVisible}");
		if (spellBar.Visible != _spellBarExpectVisible)
			_failures.Add($"法术栏显示错误 spellBarVisible={spellBar.Visible} 期望={_spellBarExpectVisible}");
		if (_spellBarExpectVisible && slots.GetChildCount() != 1)
			_failures.Add($"法术栏槽位数量错误 count={slots.GetChildCount()}");
	}

	private Vector2 GetEmptyScreenPoint()
	{
		Vector2 world = _playerCastle.GlobalPosition + _playerCastle.GetCellCenter(6, 6);
		return GetViewport().GetCanvasTransform() * world;
	}

	private void PushBattlefieldClick(Vector2 screenPoint)
	{
		if (!_inputScaleReady)
		{
			InputEventMouseButton probe = new()
			{
				ButtonIndex = MouseButton.Left,
				Pressed = true,
				Position = Vector2.One,
				GlobalPosition = Vector2.One,
			};
			GetViewport().PushInput(probe);
		}
		Vector2 inputPosition = screenPoint / _inputScale;
		InputEventMouseMotion motion = new()
		{
			Position = inputPosition,
			GlobalPosition = screenPoint,
		};
		GetViewport().PushInput(motion);
		InputEventMouseButton click = new()
		{
			ButtonIndex = MouseButton.Left,
			Pressed = true,
			Position = inputPosition,
			GlobalPosition = screenPoint,
		};
		GetViewport().PushInput(click);
		InputEventMouseButton release = new()
		{
			ButtonIndex = MouseButton.Left,
			Pressed = false,
			Position = inputPosition,
			GlobalPosition = screenPoint,
		};
		GetViewport().PushInput(release);
	}

	private void CheckRitualPhaseGate()
	{
		switch (_ritualGateStep)
		{
			case 0:
				StartRitualPhaseCheck();
				break;
			case 1:
				VerifyRitualTicksInCombat();
				break;
			case 2:
				VerifyRitualFrozenInShop();
				break;
			case 3:
				VerifyRitualFrozenInCombine();
				break;
		}
	}

	private void StartRitualPhaseCheck()
	{
		GameManager.Get<BlackMagicService>().Add(20);
		if (_darkAltar == null || !_darkAltar.TryStartRitual(RitualRules.BlackMagicChargeId))
		{
			_failures.Add("阶段门控检查无法开始仪式");
			ReportAndQuit();
			return;
		}
		_ritualRemainingAtStart = _darkAltar.RitualRemainingSeconds;
		AdapterRegistry.Resolve<GameManager>().AdvancePhase();
		_ritualGateStep = 1;
		_stepStartedAt = _elapsed;
	}

	private void VerifyRitualTicksInCombat()
	{
		if (_elapsed - _stepStartedAt < RitualTickWait)
			return;
		if (!(_darkAltar.RitualRemainingSeconds < _ritualRemainingAtStart))
			_failures.Add("战斗阶段仪式未读条");
		_ritualRemainingAtCombatEnd = _darkAltar.RitualRemainingSeconds;
		AdapterRegistry.Resolve<GameManager>().AdvancePhase();
		_ritualGateStep = 2;
		_stepStartedAt = _elapsed;
	}

	private void VerifyRitualFrozenInShop()
	{
		if (_elapsed - _stepStartedAt < RitualPauseWait)
			return;
		if (_darkAltar.RitualRemainingSeconds != _ritualRemainingAtCombatEnd)
			_failures.Add("商店阶段仪式不应读条");
		AdapterRegistry.Resolve<GameManager>().AdvancePhase();
		_ritualGateStep = 3;
		_stepStartedAt = _elapsed;
	}

	private void VerifyRitualFrozenInCombine()
	{
		if (_elapsed - _stepStartedAt < RitualPauseWait)
			return;
		if (_darkAltar.RitualRemainingSeconds != _ritualRemainingAtCombatEnd)
		{
			_failures.Add("组合阶段仪式不应读条");
			ReportAndQuit();
			return;
		}
		GD.Print("SMOKE: ritual phase gate ok");
		ReportAndQuit();
	}
	private void WaitNight()
	{
		if (!AdapterRegistry.Resolve<IGameState>().IsCombat)
			return;
		_nightObserved = true;
		_stepStartedAt = _elapsed;
		EnableFastProduction();
		_step = SmokeStep.WaitSpawns;
	}

	private void WaitSpawns()
	{
		if (_elapsed - _stepStartedAt < NightSpawnWait)
			return;
		CheckNightSpawns();
		StartStudyRitual("DarkMagicBook_BlackMagicCharge", RitualRules.BlackMagicChargeId);
		_stepStartedAt = _elapsed;
		_step = SmokeStep.WaitFirstRitual;
	}

	private void WaitFirstRitual()
	{
		if (_playerCastle.Heart == null || _playerCastle.Heart.TypeId != "RitualCircleL2")
			return;
		UpgradeTierTwoBuildings();
		StartStudyRitual("DarkMagicBook_UndeadSummon", RitualRules.UndeadSummonId);
		_stepStartedAt = _elapsed;
		_step = SmokeStep.WaitSecondRitual;
	}

	private void WaitSecondRitual()
	{
		if (_playerCastle.Heart == null || _playerCastle.Heart.TypeId != "RitualCircleL3")
			return;
		AdapterRegistry.Resolve<GameManager>().AdvancePhase();
		_stepStartedAt = _elapsed;
		_step = SmokeStep.VerifyCombine;
	}

	private void VerifyCombine()
	{
		if (AdapterRegistry.Resolve<GameManager>().CurrentPhase != MatchPhase.Combine)
		{
			if (_elapsed - _stepStartedAt < CombineCheckDelay)
				return;
			AdapterRegistry.Resolve<GameManager>().AdvancePhase();
			_stepStartedAt = _elapsed;
			return;
		}
		if (_elapsed - _stepStartedAt < CombineCheckDelay)
			return;
		if (FindBuilding("DeathTemple") == null)
			_failures.Add("组合阶段未生成死亡圣殿");
		_stepStartedAt = _elapsed;
		_step = SmokeStep.CheckRitualPhaseGate;
	}

	private void EnableFastProduction()
	{
		_tomb?.SetWorkSpeedMultiplier(WorkSpeedBoost);
		_darkAltar?.SetWorkSpeedMultiplier(WorkSpeedBoost);
		FindBuilding("BlightField")?.SetWorkSpeedMultiplier(WorkSpeedBoost);
	}

	private void StartStudyRitual(string bookCardId, string ritualId)
	{
		BlackMagicService blackMagic = GameManager.Get<BlackMagicService>();
		RitualService ritualService = GameManager.Get<RitualService>();
		blackMagic.Add(DarkMagicStudyRules.GetCost(ritualService.StudyCompletionCount));
		AdapterRegistry.Resolve<Hand>().TryAddCard(new CardData
		{
			Id = bookCardId,
			Name = "冒烟测试魔法书",
			Cost = 4,
			BuildingType = bookCardId,
			Weight = 1,
			FactionId = FactionDef.NecromancerId,
			ItemType = ShopItemTypes.DarkMagicBook,
		});
		if (_playerCastle.Heart == null || !_playerCastle.Heart.TryStartRitual(RitualRules.DarkMagicStudyId))
		{
			_failures.Add($"研习仪式无法开始 {ritualId}");
			ReportAndQuit();
			return;
		}
		GD.Print($"SMOKE: study ritual started ({ritualId})");
	}

	private void UpgradeTierTwoBuildings()
	{
		TryUpgradeBuilding("Tomb", "Necropolis");
		TryUpgradeBuilding("BlightField", "BlightMarsh");
	}

	private void TryUpgradeBuilding(string sourceTypeId, string resultTypeId)
	{
		Building source = FindBuilding(sourceTypeId);
		if (source == null)
		{
			_failures.Add($"{sourceTypeId} 缺失无法升级");
			return;
		}
		float healthRatio = source.MaxHealth > 0 ? source.Health / (float)source.MaxHealth : 1f;
		IUpgradeUseCase useCase = GameManager.Get<IUpgradeUseCase>();
		List<IBuildingState> states = _playerCastle.GetBuildingStates();
		IBuildingState upgraded = useCase.TryUpgrade(
			source, resultTypeId, healthRatio,
			AdapterRegistry.Resolve<Shop>().TrySpendGold,
			new UpgradeBuildingFactory(_playerCastle),
			RitualCircleRules.GetCoreLevel(states));
		if (upgraded == null)
			_failures.Add($"{sourceTypeId} 升级失败");
	}

	private void CheckNightSpawns()
	{
		bool hasSkeleton = false;
		bool hasDarkMage = false;
		foreach (IUnitHandle unit in GameManager.Get<IFieldUnitRepository>().AllUnits)
		{
			if (!unit.IsAlive || !unit.IsPlayerUnit)
				continue;
			if (unit.TypeId == "Skeleton")
				hasSkeleton = true;
			if (unit.TypeId == "DarkMage")
				hasDarkMage = true;
		}
		if (!hasSkeleton)
			_failures.Add("夜晚墓穴未产出骷髅兵");
		if (!hasDarkMage)
			_failures.Add("夜晚黑暗祭坛未产出暗黑法师");
		GD.Print("SMOKE: night spawns checked");
	}

	private Building FindBuilding(string typeId)
	{
		foreach (Building building in _playerCastle.GetBuildings())
		{
			if (building.TypeId == typeId)
				return building;
		}
		return null;
	}

	private void ReportAndQuit()
	{
		if (_failures.Count == 0)
		{
			GD.Print("SMOKE: PASS");
			GetTree().Quit(0);
			return;
		}
		foreach (string failure in _failures)
			GD.Print($"SMOKE: FAIL {failure}");
		GetTree().Quit(1);
	}
}