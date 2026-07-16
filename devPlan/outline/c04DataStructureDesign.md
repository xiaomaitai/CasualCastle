# 九、数据结构设计

卡牌、建筑、部队等核心数据结构见 `../dataStructures.md`。

当前数据层状态：

- `GameConfig`：已落地白天/夜晚时长、初始金币等常量
- `BuildingDefinition`（`BuildingSystem`）：名称、生命、产兵、占地、组合阶
- `CardData`：一次性放置卡，不与场上建筑绑定
- `CombineRecipe`：已落地，见 `../design/combineSystem.md`
- **M6：** `BattleReport` / `CastleSnapshot` / `BuildingSnapshot`，见 `../dataStructures.md` §战报

---
