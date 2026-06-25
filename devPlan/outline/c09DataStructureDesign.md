# 九、数据结构设计

卡牌、建筑、部队等核心数据结构见 `../dataStructures.md`。

当前数据层状态：

- `GameConfig`：已落地白天/夜晚时长、初始金币等常量
- `BuildingDefinition`（`BuildingSystem`）：已落地名称、生命、产兵、占地、`HasNightCombat`；M5 扩展强化建筑与融合阶
- `CardData`：已落地运行时结构；**一次性放置卡**，打出后消耗，不与场上建筑绑定
- `FusionRecipe`：M5 落地，见 `../fusionSystemDesign.md` 与 `../dataStructures.md`
- `UnitData`：产兵属性暂由 `BuildingDefinition` 承载；独立 `UnitData` 资源可后续拆分

---
