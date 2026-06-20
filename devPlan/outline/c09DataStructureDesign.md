# 九、数据结构设计

卡牌、建筑、部队等核心数据结构见 `../dataStructures.md`。

当前数据层状态：

- `GameConfig`：已落地白天/夜晚时长、初始金币等常量
- `BuildingData`：M2 建议落地，包含名称、花费、产出间隔、产出单位预制体、`HasNightCombat`
- `CardData`：M2 建议落地，关联 `BuildingData`、卡牌图标和购买费用
- `UnitData`：M4 前落地，包含 `HasNightCombat` 等单位词条

---
