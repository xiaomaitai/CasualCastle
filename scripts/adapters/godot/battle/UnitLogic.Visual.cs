// 单位逻辑表现：隐身、增益文本、选中与睡眠视觉
using System.Collections.Generic;
using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Battle;
using CasualCastle.Domain.Building;
using CasualCastle.Domain.Shared;
using Godot;

public partial class UnitLogic
{
	private void UpdateSleepVisual()
	{
		_visual?.UpdateSleepVisual(IsActive, IsSleeping);
	}

	private void UpdateStealthVisual()
	{
		bool stealthed = _unit != null && _unit.IsStealthed;
		_visual?.SetStealthVisual(stealthed);
		UpdateBuffs();
	}

	private void UpdateBuffs()
	{
		List<string> buffs = new();
		if (_unit != null && _unit.IsStealthed)
			buffs.Add("隐身");
		if (_unit != null && _unit.ShieldHealth > 0)
			buffs.Add("护盾");
		if (IsFurious)
			buffs.Add("狂怒");
		if (_unit != null && _unit.IsAmbushing)
			buffs.Add("伏击");
		if (AuraAttackSpeedMultiplier > 1.01f)
			buffs.Add($"攻速光环 +{Mathf.RoundToInt((AuraAttackSpeedMultiplier - 1f) * 100f)}%");
		if (AuraMoveSpeedMultiplier > 1.01f)
			buffs.Add($"移速光环 +{Mathf.RoundToInt((AuraMoveSpeedMultiplier - 1f) * 100f)}%");
		if (AuraAttackDamageMultiplier > 1.01f)
			buffs.Add($"攻击光环 +{Mathf.RoundToInt((AuraAttackDamageMultiplier - 1f) * 100f)}%");
		if (AuraRegenPerSecond > 0f)
			buffs.Add("回血光环");
		if (AuraExecuteDamageMultiplier > 1.01f)
			buffs.Add("斩杀光环");
		if (AuraArmorBonus > 0)
			buffs.Add($"护甲光环 +{AuraArmorBonus}");
		_visual?.SetBuffs(buffs.ToArray());
	}

	public void SetBaseSpriteModulate(Color color)
	{
		_visual?.SetBaseModulate(color);
		UpdateSleepVisual();
	}

	public void SetSelected(bool selected)
	{
		IsSelected = selected;
		_visual.SetSelected(selected);
	}

}
