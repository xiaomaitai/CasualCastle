// 状态效果组件：减速/中毒/诅咒、战斗计时、伏击与再生结算，由 Unit 逐帧驱动
using CasualCastle.Domain.Shared;

namespace CasualCastle.Domain.Battle;

public sealed class StatusEffects
{
	private readonly Unit _unit;

	private float _kiteTimer;

	private float _slowRemaining;

	private float _slowFactor;

	private float _curseRemaining;

	private float _curseFactor;

	private float _poisonDamagePerSecond;

	private float _poisonRemaining;

	private float _poisonAccumulator;

	private float _timeSinceLastCombat;

	private float _auraRegenAccumulator;

	private float _wildHeartAccumulator;

	public StatusEffects(Unit unit)
	{
		_unit = unit;
	}

	public void NotifyCombat()
	{
		_timeSinceLastCombat = 0f;
	}

	public void ApplySlow(float slowValue, float duration)
	{
		if (slowValue <= 0f || duration <= 0f)
			return;
		_slowFactor = slowValue;
		_slowRemaining = duration;
	}

	public void ApplyPoison(float damagePerSecond, float duration)
	{
		if (damagePerSecond <= 0f || duration <= 0f)
			return;
		_poisonDamagePerSecond = damagePerSecond;
		_poisonRemaining = duration;
		_poisonAccumulator = 0f;
	}

	public void ApplyCurse(float factor, float duration)
	{
		if (factor <= 0f || duration <= 0f)
			return;
		_curseFactor = factor;
		_curseRemaining = duration;
	}

	public float GetCurseDamageFactor()
	{
		return _curseRemaining > 0f ? 1f - _curseFactor : 1f;
	}

	public void MarkKite(float duration)
	{
		_kiteTimer = duration;
	}

	public float ApplyDebuffMultipliers(float speedMult, SkillSet skills)
	{
		if (_kiteTimer > 0f)
		{
			SpecialConfig kiting = skills.GetSpecial(SpecialBehavior.Kiting);
			if (kiting != null)
				speedMult *= kiting.SpeedMult;
		}
		if (_slowRemaining > 0f)
			speedMult *= 1f - _slowFactor;
		return speedMult;
	}

	public void Tick(float dt)
	{
		if (_kiteTimer > 0f)
			_kiteTimer -= dt;
		if (_slowRemaining > 0f)
			_slowRemaining -= dt;
		if (_curseRemaining > 0f)
			_curseRemaining -= dt;
		TickPoison(dt);
		_timeSinceLastCombat += dt;
		UpdateAmbushState();
		ApplyWildHeartRegen(dt);
		ApplyAuraRegen(dt);
	}

	private void UpdateAmbushState()
	{
		SpecialConfig ambush = _unit.Skills != null ? _unit.Skills.GetSpecial(SpecialBehavior.Ambush) : null;
		if (ambush != null && _timeSinceLastCombat >= ambush.Param)
			_unit.SetAmbushing(true);
	}

	private void ApplyWildHeartRegen(float dt)
	{
		SpecialConfig regen = _unit.Skills != null ? _unit.Skills.GetSpecial(SpecialBehavior.Regen) : null;
		if (regen == null || _timeSinceLastCombat < regen.Param)
			return;
		if (_unit.Health >= _unit.MaxHealth)
		{
			_wildHeartAccumulator = 0f;
			return;
		}
		_wildHeartAccumulator += _unit.MaxHealth * GameRules.WildHeartRegenPercentPerSecond * dt;
		int amount = (int)_wildHeartAccumulator;
		if (amount <= 0)
			return;
		_wildHeartAccumulator -= amount;
		_unit.Heal(amount);
	}

	private void ApplyAuraRegen(float dt)
	{
		if (_unit.GameCtx.AuraRegenPerSecond <= 0f)
			return;
		if (_unit.Health >= _unit.MaxHealth)
		{
			_auraRegenAccumulator = 0f;
			return;
		}
		_auraRegenAccumulator += _unit.MaxHealth * _unit.GameCtx.AuraRegenPerSecond * dt;
		int amount = (int)_auraRegenAccumulator;
		if (amount <= 0)
			return;
		_auraRegenAccumulator -= amount;
		_unit.Heal(amount);
	}

	private void TickPoison(float dt)
	{
		if (_poisonRemaining <= 0f)
			return;
		_poisonRemaining -= dt;
		_poisonAccumulator += _poisonDamagePerSecond * dt;
		int poisonDamage = (int)_poisonAccumulator;
		if (poisonDamage > 0)
		{
			_poisonAccumulator -= poisonDamage;
			_unit.TakeDamage(poisonDamage, null, _unit.GameX, _unit.GameY);
		}
		if (_poisonRemaining <= 0f)
		{
			_poisonDamagePerSecond = 0f;
			_poisonAccumulator = 0f;
		}
	}
}
