# 八、数据结构设计

卡牌、建筑、部队等核心数据结构见 `../dataStructures.md`。

M1 建议先落地：

- `GameConfig`：白天/夜晚时长、初始金币、商店槽位数
- `BuildingData`：名称、花费、产出间隔、产出单位预制体、`HasNightCombat`
- `CardData`：关联 `BuildingData`、卡牌图标
- `UnitData`：`HasNightCombat`（士兵夜战词条）

---
