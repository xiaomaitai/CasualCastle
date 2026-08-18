// 建筑运行时聚合：生命、操作状态、生产循环与活动仪式的领域不变量
using System;

namespace CasualCastle.Domain.Building;

public class BuildingRuntime
{
	private int _health;
	private int _maxHealth;
	private bool _manuallyPaused;
	private bool _combineProhibited;
	private bool _workActive;
	private bool _workPaused;
	private float _workDone;
	private float _workRequired;
	private float _productionRate;
	private ActiveRitual _activeRitual;

	public event Action<int, int> HealthChanged;

	public int Health => _health;
	public int MaxHealth => _maxHealth;
	public bool IsDestroyed => _health <= 0;
	public bool IsManuallyPaused => _manuallyPaused;
	public bool IsCombineProhibited => _combineProhibited;
	public bool IsOperational => _health > 0 && !_manuallyPaused;
	public bool IsDamaged => _health < _maxHealth;
	public bool HasWorkCycle => _workActive;
	public bool WorkPaused => _workPaused;
	public bool HasProduction => _workRequired > 0f;
	public float WorkDone => _workDone;
	public float WorkRequired => _workRequired;
	public float ProductionRate => _productionRate;
	public ActiveRitual ActiveRitual => _activeRitual;
	public bool IsRitualInProgress => _activeRitual != null;

	public void Restore(int health, int maxHealth, bool manuallyPaused, bool combineProhibited)
	{
		_maxHealth = Math.Max(1, maxHealth);
		_health = Math.Clamp(health, 0, _maxHealth);
		_manuallyPaused = manuallyPaused;
		_combineProhibited = combineProhibited;
		HealthChanged?.Invoke(_health, _maxHealth);
	}

	public void SetPaused(bool paused)
	{
		_manuallyPaused = paused;
	}

	public void SetCombineProhibited(bool prohibited)
	{
		_combineProhibited = prohibited;
	}

	public void TakeDamage(int amount)
	{
		if (amount <= 0 || _health <= 0)
			return;
		_health = Math.Max(0, _health - amount);
		HealthChanged?.Invoke(_health, _maxHealth);
	}

	public void Heal(int amount)
	{
		if (amount <= 0 || _health <= 0 || _health >= _maxHealth)
			return;
		_health = Math.Min(_maxHealth, _health + amount);
		HealthChanged?.Invoke(_health, _maxHealth);
	}

	public void ApplyHealthRatio(float ratio)
	{
		_health = Math.Clamp((int)Math.Round(_maxHealth * ratio), 1, _maxHealth);
		HealthChanged?.Invoke(_health, _maxHealth);
	}

	public void BeginWork(float rate, float required)
	{
		if (rate <= 0f || required <= 0f)
			return;
		_productionRate = rate;
		_workRequired = required;
		_workDone = 0f;
		_workActive = true;
		_workPaused = false;
	}

	public void PauseWork()
	{
		_workPaused = true;
	}

	public void ResumeWork()
	{
		_workPaused = false;
	}

	public void StopWork()
	{
		_workActive = false;
		_workPaused = false;
		_workDone = 0f;
		_workRequired = 0f;
		_productionRate = 0f;
	}

	public int TickWork(float delta, float speedMultiplier)
	{
		if (!_workActive || _workPaused || _health <= 0 || _manuallyPaused)
			return 0;
		if (delta <= 0f || speedMultiplier <= 0f)
			return 0;

		_workDone += delta * _productionRate * speedMultiplier;
		int cycles = 0;
		while (_workDone >= _workRequired)
		{
			_workDone -= _workRequired;
			cycles++;
		}
		return cycles;
	}

	public void StartRitual(string ritualId, float totalSeconds, bool isCoreHosted, string bookCardId, string bookRitualId)
	{
		if (string.IsNullOrEmpty(ritualId) || totalSeconds <= 0f)
			return;
		_activeRitual = new ActiveRitual(ritualId, totalSeconds, isCoreHosted, bookCardId, bookRitualId);
	}

	public bool TickRitual(float delta)
	{
		if (_activeRitual == null || _activeRitual.IsComplete)
			return false;
		if (delta <= 0f)
			return false;

		_activeRitual.RemainingSeconds -= delta;
		return _activeRitual.IsComplete;
	}

	public void CompleteRitual()
	{
		_activeRitual = null;
	}
}
