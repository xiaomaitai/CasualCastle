# 九、数据结构设计

卡牌、建筑、部队等核心数据结构见 `../dataStructures.md`。

当前数据层状态：

- `GameConfig`：已落地白天/夜晚时长、初始金币等常量
- `BuildingDefinition`（`BuildingSystem`）：名称、生命、产兵、占地、`HasNightCombat`、融合阶
- `CardData`：已落地运行时结构；**一次性放置卡**，不与场上建筑绑定
- `FusionRecipe`：已落地，见 `../fusionSystemDesign.md`
- `UnitData`：产兵属性暂由 `BuildingDefinition` 承载
- **M6 规划：** 敌方经济可复用 `CardData` + 独立金币池，不必新建资源类型

---
