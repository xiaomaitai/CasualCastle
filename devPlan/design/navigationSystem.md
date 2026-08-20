# 寻路系统：流场寻路 + 局部转向（星际争霸 2 式）

本文取代 `rtsPushApart.md`，是单位移动的唯一依据。目标：大规模部队下依然平滑、抗堵塞、自然绕障与侧面包抄。

## 核心思路

- **全局路径用流场（Flow Field）**：以目标格子为源点反推代价场与方向场，所有奔向同一目标的单位共享同一张流场，避免逐单位寻路。
- **局部移动用转向（Steering）**：流场方向 + 软分离 + 到达刹车 + 贴墙滑动，保证个体平滑、不重叠、不穿建筑。
- **移除 Godot `NavigationAgent2D` / `NavigationRegion2D` / RVO**：寻路与移动全部下沉到 domain 层纯 C#，可单测、可调参。
- 单位状态机只负责"要不要移动、往哪移动"（`MoveGoal`），移动本身由 `NavigationService` 统一驱动。

## 术语

- **导航格子（nav cell）**：边长 50 游戏单位（半个建筑格子），战场 3840×2160 游戏单位 → 约 77×44 格。
- **流场（FlowField）**：每个非阻塞格存 `cost`（到目标的距离代价）与 `dir`（指向代价更小邻居的方向）。
- **阻塞格**：敌方**非核心、未摧毁**建筑占用的格子，以及战场边界外。
- **MoveGoal**：单位当前移动目标（游戏单位坐标）；无目标（`HasMoveGoal=false`）则原地站定。

## 流场计算（FlowFieldService）

1. 以目标格为源点，8 连通（正交代价 1，对角 √2）Dijkstra 向外扩散。
2. 每格方向 = 指向代价最小的邻居的方向；源点格方向为零（到达即停）。
3. 阻塞格不扩散、无方向；被围死的目标附近格不可达。
4. 目标格被阻塞时，先吸附到最近的开放格再计算。
5. 按「阵营 × 目标格」缓存流场；建筑变更（放置/摧毁/销毁）时版本号递增，清空缓存。

## 局部转向（SteeringService）

每帧对每个**有移动目标**的存活单位执行：

1. **流向采样**：对当前格及相邻 4 格的方向做双线性插值，归一化得流动方向（保证跨格时方向连续、移动平滑）。
2. **到达刹车**：距目标小于 `ArrivalRadius` 时停止，防止绕圈。
3. **软分离**：查询空间哈希内邻近单位，沿连线推离，力度随距离线性衰减；影响半径 = 双方碰撞半径和 × `SeparationRadiusMultiplier`。
4. **速度平滑**：`vel = lerp(vel, desired, k*dt)`，限制单帧加速度，去抖动。
5. **贴墙滑动**：目标位置所在格为阻塞时，按轴取消进入墙的分量，沿墙滑行。
6. **硬去重叠**：对位置已侵入对方碰撞半径和之内的单位对，按半径占比加权将当前单位推开（每帧总位移上限 `MaxOverlapPush`，落在阻塞格则跳过），保证永不重叠。
7. 位置夹紧在战场边界内；`pos += vel * dt`。

## 单位-单位避让

- 所有存活单位进入空间哈希（格宽 128 游戏单位）。
- **接战/攻城/治疗/等待单位**（无 MoveGoal）不移动，但作为**静态障碍**：行军单位经分离绕开它们，形成水流式侧面包抄。
- 仅行军单位之间施加软分离；接战单位不被推开、也不主动推开别人。

## 单位-建筑避障

- 敌方非核心未摧毁建筑 → 阻塞格，流场自然绕行（不再用"每帧推出"）。
- 核心建筑（城堡）不阻塞：单位可流入核心区域，重叠后转攻城。
- 已摧毁建筑不阻塞，单位可直接穿过。

## 单位移动契约

| 状态 | MoveGoal |
|------|----------|
| 行军 Marching | 行军目标（城堡/巡逻中心/酒馆站位） |
| 交战/反击 Fighting/Retaliating | 射程外 → 目标单位位置；射程内 → 无（站定） |
| 攻城 Sieging | 无（站定于建筑旁） |
| 治疗 Healing | 射程外 → 治疗目标位置；射程内 → 无（站定） |
| 等待 Waiting | 无（站定） |

## 模块归属

- **domain/Battle**（纯 C#，零 Godot）：`NavGrid`、`FlowField`、`FlowFieldService`、`SteeringService`、`NavigationService`、`NavSpatialHash`、端口 `INavGridSource` / `INavigationService`；单位移动契约由 `Unit` 提供 `MoveGoal`。
- **adapters/godot/battle**：`NavGridSource` 从 `IFieldUnitRepository.AllBuildings` 与 `BattlefieldMetrics` 构建双方阻塞网格。
- **BattleManager._Process** 每帧调用 `INavigationService.Step(dt)`（仅战斗阶段）。

## 参数速查

| 参数 | 值 | 说明 |
|------|----|------|
| nav cell 边长 | 50 | 半格粒度，兼顾平滑与开销 |
| ArrivalRadius | 8 | 到达停止半径（游戏单位） |
| SeparationRadiusMultiplier | 1.6 | 软分离影响半径 = 碰撞半径和 × 该系数 |
| MaxOverlapPush | 24 | 硬去重叠每帧位移上限 |
| 速度平滑系数 k | 12 | 越大越跟手 |
| 空间哈希格宽 | 128 | 邻居查询 |

## 移除项

- `NavigationAgent2D`（`prefabs/Unit.tscn`）、`NavigationRegion2D`（`main_game.tscn`）
- `NavigationPortAdapter`、`INavigationPort`、`RvoService`/`IRvoService`、`UnitSpatialService`/`IUnitSpatialService`、`TrapNavPort`
- 单位-建筑/单位-单位每帧推开逻辑（由流场绕行 + 软分离取代）
