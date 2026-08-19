// 单位表现模型：只读缓冲展示标签，领域状态不含本地化文案
using Godot;
using System.Collections.Generic;

public sealed class UnitPresentationModel
{
	public string[] GetBuffLabels(bool isStealthed, bool hasShield, bool isFurious, bool isAmbushing,
		float auraAttackSpeed, float auraMoveSpeed, float auraAttackDamage, float auraRegen, float auraExecute, int auraArmor)
	{
		List<string> buffs = new();
		if (isStealthed)
			buffs.Add("隐身");
		if (hasShield)
			buffs.Add("护盾");
		if (isFurious)
			buffs.Add("狂怒");
		if (isAmbushing)
			buffs.Add("伏击");
		if (auraAttackSpeed > 1.01f)
			buffs.Add($"攻速光环 +{Mathf.RoundToInt((auraAttackSpeed - 1f) * 100f)}%");
		if (auraMoveSpeed > 1.01f)
			buffs.Add($"移速光环 +{Mathf.RoundToInt((auraMoveSpeed - 1f) * 100f)}%");
		if (auraAttackDamage > 1.01f)
			buffs.Add($"攻击光环 +{Mathf.RoundToInt((auraAttackDamage - 1f) * 100f)}%");
		if (auraRegen > 0f)
			buffs.Add("回血光环");
		if (auraExecute > 1.01f)
			buffs.Add("斩杀光环");
		if (auraArmor > 0)
			buffs.Add($"护甲光环 +{auraArmor}");
		return buffs.ToArray();
	}
}
