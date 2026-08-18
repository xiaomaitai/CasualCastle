// 生产组件：产兵工作循环，完成时发出生成请求（Building 只订阅并实例化）
using CasualCastle.Domain.Battle;
using CasualCastle.Domain.Building;
using Godot;
using System;

public sealed class ProductionComponent
{
	public event Action<string, int> SpawnRequested;

	private readonly Building _owner;
	private readonly BuildingRuntime _runtime;
	private readonly BuildingSystem _buildingSystem;

	private float _speedMultiplier = 1f;

	public ProductionComponent(Building owner, BuildingRuntime runtime, BuildingSystem buildingSystem)
	{
		_owner = owner;
		_runtime = runtime;
		_buildingSystem = buildingSystem;
	}

	public float SpeedMultiplier => _speedMultiplier;

	public void Tick(float delta)
	{
		if (!_runtime.HasWorkCycle || _runtime.WorkPaused || !_owner.CanWork)
			return;

		int cycles = _runtime.TickWork(delta, _speedMultiplier * DungeonFactionBuffs.ProductionSpeedMultiplier);
		if (cycles > 0)
			SpawnRequested?.Invoke(_owner.TypeId, cycles);
	}

	public void SetSpeedMultiplier(float multiplier)
	{
		_speedMultiplier = Mathf.Max(0.1f, multiplier);
	}

	public void RefreshCycle()
	{
		if (_owner.CanWork)
		{
			if (_runtime.WorkPaused)
				Resume();
			else if (!_runtime.HasWorkCycle)
				Start();
		}
		else
		{
			Pause();
		}
	}

	public void Start()
	{
		float productionRate = _buildingSystem.GetProductionRate(_owner.TypeId);
		if (productionRate <= 0f)
			return;

		int unitCost = _buildingSystem.GetUnitCost(_owner.TypeId);
		if (unitCost <= 0)
			return;

		if (!_runtime.HasWorkCycle)
			_owner.OnWorkStarted();
		_runtime.BeginWork(productionRate, unitCost);
	}

	public void Pause()
	{
		if (!_runtime.HasWorkCycle || _runtime.WorkPaused)
			return;

		_runtime.PauseWork();
		_owner.OnWorkPaused();
	}

	public void Resume()
	{
		if (!_runtime.HasWorkCycle || !_runtime.WorkPaused)
			return;

		_runtime.ResumeWork();
		if (!_owner.CanWork)
			return;

		_owner.OnWorkResumed();
	}

	public void Stop()
	{
		_runtime.StopWork();
		_owner.OnWorkStopped();
	}
}
