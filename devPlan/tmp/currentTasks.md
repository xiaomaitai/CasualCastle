# 当前任务：亡灵派系 v0.5（高阶领主线 + 完整祭仪 + 死亡天使）

v0.4 已验收（无头冒烟实测通过，提交 7588f59）。v0.5 数值以 necromancerFaction.md 设计定稿为准（首版占位，实测后调优）。

## 一、设计定稿（文档）

- [x] necromancerFaction.md 补 v0.5 数值：T4/T5 建筑与单位、死亡天使、新增仪式书、死者之门升级路径说明
- [x] 验收：文档含全部 v0.5 数值表与获取方式

## 二、光环框架扩展（吸血 / 驱散抵抗）

- [x] AuraEffects / AuraBonusTypes 新增 lifesteal（吸血）、dispel_resist（驱散抵抗）
- [x] AuraService 结算新加成；GameContext / SkillService 传递
- [x] Unit 攻击后按吸血比例回复；驱散伤害按抵抗折减（0.5 → ×1.5，1.0 → ×1 免疫）
- [x] 单测：吸血/驱散抵抗数值、非亡灵不受影响、稳定光环免疫驱散

## 三、T4/T5 建筑与单位（领主线）

- [x] 数据层：BloodAltar（升级：死亡圣殿，核心 Lv≥4）、NecromancySanctum（升级：鲜血祭坛，核心 Lv≥5）、DeathGate（组合：鲜血祭坛+黑暗祭坛，核心 Lv≥5）
- [x] 单位：BloodKnight（吸血光环 15%）、NecromancyInstructor（反魔法护罩 50%）、DeathLord（稳定光环 100% 免疫）
- [x] 技能定义 + unit_skills 挂接 + 技术树/配方接线
- [x] 单测：组合门槛、升级门槛、光环数值

## 四、完整祭仪（核心 Lv5 + 死亡天使）

- [x] 新增仪式：死亡天使降临（死亡天使之书）、亡灵巫术研习（亡灵巫术秘典之书）
- [x] RitualReward 扩展 UndeadDamageMultiplier；核心奖励召唤死亡天使 / 永久亡灵伤害 +10%
- [x] 商店：2 本魔法书入 item_catalog；死亡天使单位（穿刺伤害）
- [x] 单测：仪式前置/消耗/奖励、研习至核心 Lv5、永久伤害加成生效

## 五、素材与导入

- [x] 7/7 素材已生成并导入：BloodKnight/NecromancyInstructor/DeathLord/DeathAngel 主图 + 吸血/反魔法护罩/稳定光环图标
- [x] godot-import 无头导入生成 .import/.uid

## 六、收尾

- [x] dotnet build 零 warning、单测全过（453）
- [x] 无头冒烟实测（数据/新光环/新仪式/组合与升级门槛）并提交
- [x] 更新 necromancerFaction.md 实现阶段状态