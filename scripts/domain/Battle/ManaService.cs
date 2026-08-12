// 奸奇魔力资源服务：魔力建筑持续产出，供魔王释放魔力系技能
using System;

namespace CasualCastle.Domain.Battle;

public sealed class ManaService
{
	public const float MaxMana = 100f;

	public event Action<float> ValueChanged;

	public float Value { get; private set; }

	public void Add(float amount)
	{
		if (amount <= 0f || Value >= MaxMana)
			return;
		Value = Math.Min(MaxMana, Value + amount);
		ValueChanged?.Invoke(Value);
	}

	public bool TrySpend(float amount)
	{
		if (amount <= 0f || Value < amount)
			return false;
		Value -= amount;
		ValueChanged?.Invoke(Value);
		return true;
	}

	public void Reset()
	{
		if (Value == 0f)
			return;
		Value = 0f;
		ValueChanged?.Invoke(Value);
	}
}