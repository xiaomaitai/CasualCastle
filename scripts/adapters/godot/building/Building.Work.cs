// 建筑工作循环：产兵节奏、单位生成与暂停/恢复
using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Battle;
using CasualCastle.Domain.Building;
using CasualCastle.Domain.Shared;
using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class Building : Area2D
{
	public override void _Process(double delta)
	{
		TickRitual((float)delta);
		if (!_workActive || _workPaused || !CanWork)
			return;

		_workDone += (float)delta * _productionRate * _workSpeedMultiplier;
		UpdateWorkEffectFromProgress();

		if (_workDone < _workRequired)
			return;

		_workDone -= _workRequired;
		PerformWork();
		PlayWorkJumpVisual();
	}

	protected virtual void OnPhaseChanged(MatchPhase phase)
	{
		UpdateWorkCycle();
	}

	private void OnFastProductionChanged()
	{
		SetWorkSpeedMultiplier(CheatState.FastProductionEnabled ? 10f : 1f);
	}

	protected void UpdateWorkCycle()
	{
		if (CanWork)
		{
			if (_workPaused)
				ResumeWork();
			else if (!_workActive)
				StartWorkCycle();
		}
		else
		{
			PauseWork();
		}
	}

	public void SetWorkSpeedMultiplier(float multiplier)
	{
		multiplier = Mathf.Max(0.1f, multiplier);
		if (Mathf.IsEqualApprox(_workSpeedMultiplier, multiplier))
			return;

		_workSpeedMultiplier = multiplier;
		UpdateHordeIndicator();
	}

	private void StartWorkCycle()
	{
		float productionRate = BuildingSys.GetProductionRate(TypeId);
		if (productionRate <= 0f)
			return;

		int unitCost = BuildingSys.GetUnitCost(TypeId);
		if (unitCost <= 0)
			return;

		_productionRate = productionRate;
		BeginWork(unitCost);
	}

	private void PerformWork()
	{
		SpawnUnits(1);
	}

	private void SpawnUnits(int count)
	{
		if (!CanWork || _battlefield == null || CastleRef == null || count <= 0)
			return;
		if (_gameManager?.CurrentState == GameManager.GameState.GameOver)
			return;

		PackedScene unitScene = GD.Load<PackedScene>("res://prefabs/Unit.tscn");
		if (unitScene == null)
			return;

		for (int i = 0; i < count; i++)
		{
			Unit shell = unitScene.Instantiate<Unit>();
			UnitLogic unit = shell.GetNode<UnitLogic>("Logic");
			unit.IsPlayerUnit = _isPlayerBuilding;
			BuildingSys.ApplyUnitSpawnStats(TypeId, unit);
			ApplyFactionBuff(unit);
			UnitSpawn.PlaceUnit(
				_battlefield, CastleRef, shell,
				BuildingSys.GetFootprint(TypeId), GridX, GridY, _spawnCount);
			_spawnCount++;
			if (_isPlayerBuilding)
				AssignToTavern(unit);
		}
	}

	private void SpawnUnitType(string unitTypeId, int count)
	{
		if (_battlefield == null || CastleRef == null || count <= 0 || string.IsNullOrEmpty(unitTypeId))
			return;
		if (_gameManager?.CurrentState == GameManager.GameState.GameOver)
			return;

		PackedScene unitScene = GD.Load<PackedScene>("res://prefabs/Unit.tscn");
		if (unitScene == null)
			return;

		IUnitRepository unitRepo = GameManager.Get<IUnitRepository>();
		for (int i = 0; i < count; i++)
		{
			Unit shell = unitScene.Instantiate<Unit>();
			UnitLogic unit = shell.GetNode<UnitLogic>("Logic");
			unit.IsPlayerUnit = _isPlayerBuilding;
			unit.InitializeFromStats(unitRepo.Get(unitTypeId));
			ApplyFactionBuff(unit);
			UnitSpawn.PlaceUnit(
				_battlefield, CastleRef, shell,
				BuildingSys.GetFootprint(TypeId), GridX, GridY, _spawnCount);
			_spawnCount++;
			if (_isPlayerBuilding)
				AssignToTavern(unit);
		}
	}

	private static void ApplyFactionBuff(UnitLogic unit)
	{
		if (unit.Handle != null && UndeadRules.IsUndead(unit.Handle.Race) && UndeadFactionBuffs.MaxHealthBonus > 0)
			unit.Handle.AddMaxHealthBonus(UndeadFactionBuffs.MaxHealthBonus);
	}

	private void AssignToTavern(UnitLogic unit)
	{
		TavernPartyService tavernParty = GameManager.Get<TavernPartyService>();
		IFieldUnitRepository fieldRepo = GameManager.Get<IFieldUnitRepository>();
		if (tavernParty == null || fieldRepo == null)
			return;
		tavernParty.TryAssignTavern(unit.Handle, fieldRepo.AllBuildings);
	}

	protected void BeginWork(float workRequired)
	{
		_workRequired = workRequired;

		if (_workActive)
			return;

		_workActive = true;
		_workPaused = false;
		_workDone = 0f;
		SetProcess(true);
	}

	protected void PauseWork()
	{
		if (!_workActive || _workPaused)
			return;

		_workPaused = true;

		if (_jumpTween != null && _jumpTween.IsValid() && _jumpTween.IsRunning())
		{
			_jumpTween.Pause();
			_jumpTweenAwaitingResume = true;
		}
	}

	protected void ResumeWork()
	{
		if (!_workActive || !_workPaused)
			return;

		_workPaused = false;
		if (!CanWork)
			return;

		if (_jumpTweenAwaitingResume && _jumpTween != null && _jumpTween.IsValid())
		{
			_jumpTweenAwaitingResume = false;
			_jumpTween.Play();
		}
	}

	private void StopWork()
	{
		_workActive = false;
		_workPaused = false;
		_workDone = 0f;
		_jumpTweenAwaitingResume = false;
		SetProcess(false);
		CancelJumpTween();
		ResetWorkVisual();
	}

}
