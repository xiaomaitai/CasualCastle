// 魔王舒适度服务：行动消耗、距离惩罚与宝座附近恢复
using System;

namespace CasualCastle.Domain.Battle;

public sealed class ComfortService
{
	public event Action<float> ValueChanged;

	public float Value { get; private set; } = DemonKingRules.MaxComfort;

	public float MaxComfort { get; private set; } = DemonKingRules.MaxComfort;

	public float RestoreMultiplier { get; set; } = 1f;

	public float AttackCostMultiplier { get; set; } = 1f;

	public float HitCostMultiplier { get; set; } = 1f;

	public float GlobalCostMultiplier { get; set; } = 1f;

	public bool IsCombatReady => Value >= DemonKingRules.CombatResumeThreshold;

	public void SpendAttack()
	{
		Spend(DemonKingRules.AttackComfortCost * AttackCostMultiplier * GlobalCostMultiplier);
	}

	public void SpendHit()
	{
		Spend(DemonKingRules.HitComfortCost * HitCostMultiplier * GlobalCostMultiplier);
	}

	public bool TrySpend(float amount)
	{
		if (amount <= 0f || Value < amount)
			return false;
		Value -= amount;
		ValueChanged?.Invoke(Value);
		return true;
	}

	public void TickRestore(float dt, bool nearThrone, bool inCombat)
	{
		if (inCombat || dt <= 0f)
			return;
		float rate = nearThrone ? DemonKingRules.RestorePerSecondNearThrone : DemonKingRules.RestorePerSecondIdle;
		Add(rate * RestoreMultiplier * dt);
	}

	public void Spend(float amount)
	{
		if (amount <= 0f || Value <= 0f)
			return;
		Value = Math.Max(0f, Value - amount);
		ValueChanged?.Invoke(Value);
	}

	public void TickDistancePenalty(float dt, float distance, float patrolRadius)
	{
		if (dt <= 0f || distance <= patrolRadius || patrolRadius <= 0f)
			return;
		float excess = (distance - patrolRadius) / patrolRadius;
		Spend(DemonKingRules.DistanceComfortCostPerSecond * excess * dt * GlobalCostMultiplier);
	}

	public void SetMaxComfort(float maxComfort)
	{
		if (maxComfort <= 0f || Math.Abs(MaxComfort - maxComfort) < 0.01f)
			return;
		MaxComfort = maxComfort;
		if (Value > MaxComfort)
		{
			Value = MaxComfort;
			ValueChanged?.Invoke(Value);
		}
	}

	public void Reset()
	{
		MaxComfort = DemonKingRules.MaxComfort;
		RestoreMultiplier = 1f;
		AttackCostMultiplier = 1f;
		HitCostMultiplier = 1f;
		GlobalCostMultiplier = 1f;
		if (Value == DemonKingRules.MaxComfort)
			return;
		Value = DemonKingRules.MaxComfort;
		ValueChanged?.Invoke(Value);
	}

	private void Add(float amount)
	{
		if (amount <= 0f || Value >= MaxComfort)
			return;
		Value = Math.Min(MaxComfort, Value + amount);
		ValueChanged?.Invoke(Value);
	}
}