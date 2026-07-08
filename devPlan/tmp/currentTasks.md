# P3 人类种族实现

## 目标

6 座人类建筑（3 T1 + 3 T2）+ 6 种人类兵种，全部通过 `config.db` 配置；商店卡池改为加权随机；建筑视觉定义适配 2×2 占地。

---

## 任务拆解

### 3.1 建筑数据配置（config.db → building_defs）

替换现有测试建筑数据为 6 座人类建筑（全部 2×2 占地），保留 CastleHeart：

| type_id | display_name | max_health | spawn_interval | unit_type_id | fusion_tier |
|---------|-------------|------------|---------------|-------------|-------------|
| Barracks | 兵营 | 200 | 8.0 | Spearman | 0 |
| ArcheryRange | 靶场 | 150 | 10.0 | Archer | 0 |
| Stable | 马厩 | 220 | 14.0 | Knight | 0 |
| Armory | 军府 | 300 | 7.0 | Swordsman | 1 |
| CrossbowTower | 射楼 | 220 | 9.0 | Crossbowman | 1 |
| Ranch | 牧场 | 330 | 12.0 | HeavyCavalry | 1 |

每座建筑 `footprint_json` 均为 `[[0,0],[1,0],[0,1],[1,1]]`；`collision_width/height` = 188（2×94）；`main_cell` 和 `spawn_cell` 需适配 2×2。

**验收项：**
- 删除旧的 Barracks(1×1)、ArcheryRange(2×1)、Stable(1×4)、WolfDen、BarracksT2、WolfDenT2 记录（这些是测试占位数据，人类种族不再使用）
- 新增 6 条人类建筑记录，footprint、碰撞尺寸、产兵间隔等字段正确
- CastleHeart 记录保持不变
- `SqliteBuildingRepository` 能正常加载全部 7 条（1 Castle + 6 Human）

### 3.2 单位数据配置（config.db → unit_stats）

替换现有测试单位数据为 6 种人类兵种：

| type_id | size | attack_type | damage_type | armor_type | health | damage | speed | atk_range | atk_cd | vision | night |
|---------|------|-------------|-------------|------------|--------|--------|-------|-----------|--------|--------|-------|
| Spearman | Medium(1) | Melee(0) | Normal(0) | Light(0) | 40 | 10 | 80 | 30 | 1.0 | 250 | 0 |
| Archer | Medium(1) | Ranged(1) | Pierce(1) | Light(0) | 25 | 12 | 60 | 200 | 1.5 | 300 | 0 |
| Knight | Large(2) | Melee(0) | Normal(0) | Heavy(1) | 60 | 18 | 120 | 35 | 1.2 | 280 | 0 |
| Swordsman | Medium(1) | Melee(0) | Normal(0) | Light(0) | 55 | 16 | 80 | 30 | 0.9 | 260 | 0 |
| Crossbowman | Medium(1) | Ranged(1) | Pierce(1) | Light(0) | 30 | 22 | 55 | 180 | 2.0 | 280 | 0 |
| HeavyCavalry | Large(2) | Melee(0) | Normal(0) | Heavy(1) | 85 | 28 | 110 | 35 | 1.3 | 290 | 0 |

`unit_color` 使用现有占位色值（后续素材管线替换）。

**验收项：**
- 删除旧的 Swordsman、Archer、Cavalry、Werewolf、HeavySwordsman、WerewolfLord 记录
- 新增 6 条人类单位记录，所有字段与设计表一致
- `SqliteUnitRepository` 能正常加载全部 6 条

### 3.3 商店加权随机

`ShopRules` 当前为均匀随机，扩展为加权随机。

**代码改动：**
- `CardData` 新增 `Weight` 属性
- `shop_catalog` 表新增 `weight` 列（INTEGER，默认 1）
- `GameDataLoader.LoadShopCatalog` 读取 `weight` 字段
- `ShopRules.GenerateOffers` / `RefreshOfferSlot` 按权重抽样

**商店卡池：**

| id | name | cost | building_type | weight |
|----|------|------|--------------|--------|
| barracks | 兵营 | 3 | Barracks | 40 |
| archery_range | 靶场 | 4 | ArcheryRange | 35 |
| stable | 马厩 | 5 | Stable | 25 |

**验收项：**
- `shop_catalog` 表含 3 条人类 T1 建筑卡，各带权重
- `ShopRules` 按权重随机生成卡牌（非均匀）
- 手牌购买流程正常（拖拽放置、直放均可用）
- 敌方可复刻商店购买行为（ReplayService 适配不变）

### 3.4 伤害矩阵更新（config.db → damage_matrix）

替换当前 damage_matrix 为人类种族设计矩阵：

| damage_type ↓ / armor → | Light(0) | Heavy(1) | Fortified(2) | Beast(3) |
|--------------------------|----------|----------|-------------|---------|
| Normal(0) | 1.0 | 0.8 | 0.6 | 1.0 |
| Pierce(1) | 1.2 | 0.9 | 0.5 | 0.8 |
| Siege(2) | 0.8 | 1.0 | 1.5 | 0.9 |
| Magic(3) | 1.0 | 1.2 | 1.0 | 1.0 |

**验收项：**
- `damage_matrix` 表 16 行全部按设计值更新
- `DamageMatrix.LoadFrom` 正确加载，`GetMultiplier` 返回对应倍率
- 战斗中伤害计算使用矩阵倍率（弓箭手 Pierce → Light 造成 1.2× 伤害）

### 3.5 建筑视觉定义更新

`BuildingSystem.Visuals` 字典适配人类建筑类型：所有 6 座人类建筑均为 2×2，footprint 统一，视觉通过 Modulate 颜色区分。

**验收项：**
- Visuals 字典移除旧条目（Barracks 1×1、ArcheryRange 2×1、Stable 1×4、WolfDen、BarracksT2、WolfDenT2）
- 新增 6 条人类建筑视觉定义（Barracks/ArcheryRange/Stable/Armory/CrossbowTower/Ranch），footprint 均为 2×2
- 各建筑 Modulate 颜色与兵种色系呼应（步兵线蓝灰、射手线绿、骑兵线棕，T2 略亮）
- 放置预览、碰撞体渲染与 2×2 占地一致

### 3.6 集成测试

验证人类种族从商店购买 → 放置 → 产兵 → 战斗的完整链路。

**验收项：**
- 商店展示 3 种 T1 建筑卡（兵营/靶场/马厩），按权重随机
- 购买并放置兵营 → 8s 产出枪兵 → 枪兵向敌方推进
- 购买并放置靶场 → 10s 产出弓箭手 → 弓箭手远程攻击（200 范围）
- 购买并放置马厩 → 14s 产出骑士 → 骑士高速移动（120）
- 多座建筑同时产兵，不同兵种移动速度、攻击范围差异明显
- 伤害倍率生效：弓箭手（Pierce）对骑士（Heavy）伤害降低（0.9×）
- 现有战报录制/回放功能不受影响
