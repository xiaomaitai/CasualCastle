// 黑魔法资源服务：单位阵亡时收集，己方阵亡产出高于敌方
using System;

namespace CasualCastle.Domain.Battle;

public sealed class BlackMagicService
{
	public const int FriendlyDeathYield = 25;
	public const int EnemyDeathYield = 10;

	public event Action<int> ValueChanged;

	public int Value { get; private set; }

	public void OnUnitDied(bool isFriendlyToNecromancer, string race)
	{
		if (UndeadRules.IsNonLiving(race))
			return;
		Add(isFriendlyToNecromancer ? FriendlyDeathYield : EnemyDeathYield);
	}

	public void Add(int amount)
	{
		if (amount <= 0)
			return;
		Value += amount;
		ValueChanged?.Invoke(Value);
	}

	public bool TrySpend(int amount)
	{
		if (amount <= 0 || Value < amount)
			return false;
		Value -= amount;
		ValueChanged?.Invoke(Value);
		return true;
	}

	public void Reset()
	{
		if (Value == 0)
			return;
		Value = 0;
		ValueChanged?.Invoke(Value);
	}
}