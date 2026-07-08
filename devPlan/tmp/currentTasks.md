# P3 人类种族实现 — v0.1 王国军 T1+T2

## 目标

实现人类第一个流派**王国军**：5 座 T1 建筑 + 5 座 T2 建筑 + 10 种兵种，全部通过 `config.db` 配置；商店卡池改为加权随机；邻接加速机制。

> - 完整三流派设计（王国军 / 冒险者公会 / 神殿卫士）见 `devPlan/design/humanRace.md`。
> - v0.1 仅实现王国军 T1+T2。T3 皇家兵种（5 条融合链终点）见 `devPlan/tmp/todo.md`。
> - 王国军 T1 单位单体战力有意设计为三派最低——这是流派特色的体现，不是平衡问题。

---

## 任务拆解

### 3.1 建筑数据配置（config.db → building_defs）

替换现有测试建筑数据为 10 座王国军建筑（全部 2×2 占地），保留 CastleHeart：

| type_id | display_name | max_health | spawn_interval | unit_type_id | fusion_tier |
|---------|-------------|------------|---------------|-------------|-------------|
| Barracks | 兵营 | 200 | 8.0 | Spearman | 0 |
| ShieldCamp | 盾营 | 250 | 12.0 | ShieldBearer | 0 |
| ArcheryRange | 靶场 | 150 | 10.0 | Archer | 0 |
| Stable | 马厩 | 220 | 14.0 | Knight | 0 |
| ScoutCamp | 斥候营 | 100 | 6.0 | Scout | 0 |
| Armory | 军府 | 300 | 7.0 | Swordsman | 1 |
| Bulwark | 壁垒 | 380 | 10.0 | HeavyShield | 1 |
| CrossbowTower | 射楼 | 220 | 9.0 | Crossbowman | 1 |
| Ranch | 牧场 | 330 | 12.0 | HeavyCavalry | 1 |
| RangerPost | 游骑哨 | 150 | 5.0 | LightCavalry | 1 |

每座建筑 `footprint_json` 均为 `[[0,0],[1,0],[0,1],[1,1]]`；`collision_width/height` = 188（2×94）。

**验收项：**
- 删除旧的 Barracks(1×1)、ArcheryRange(2×1)、Stable(1×4)、WolfDen、BarracksT2、WolfDenT2 记录
- 新增 10 条王国军建筑记录
- CastleHeart 记录保持不变
- `SqliteBuildingRepository` 能正常加载全部 11 条（1 Castle + 10 Army）

### 3.2 单位数据配置（config.db → unit_stats）

替换现有测试单位数据为 10 种王国军兵种：

| type_id | size | attack_type | damage_type | armor_type | health | damage | speed | atk_range | atk_cd | vision | night |
|---------|------|-------------|-------------|------------|--------|--------|-------|-----------|--------|--------|-------|
| Spearman | Medium(1) | Melee(0) | Normal(0) | Light(0) | 40 | 10 | 80 | 30 | 1.0 | 250 | 0 |
| ShieldBearer | Medium(1) | Melee(0) | Normal(0) | Heavy(1) | 70 | 5 | 45 | 30 | 1.8 | 200 | 0 |
| Archer | Medium(1) | Ranged(1) | Pierce(1) | Light(0) | 25 | 12 | 60 | 200 | 1.5 | 300 | 0 |
| Knight | Large(2) | Melee(0) | Normal(0) | Heavy(1) | 60 | 18 | 120 | 35 | 1.2 | 280 | 0 |
| Scout | Small(0) | Melee(0) | Normal(0) | Light(0) | 15 | 3 | 160 | 25 | 1.0 | 400 | 0 |
| Swordsman | Medium(1) | Melee(0) | Normal(0) | Light(0) | 55 | 16 | 80 | 30 | 0.9 | 260 | 0 |
| HeavyShield | Large(2) | Melee(0) | Normal(0) | Heavy(1) | 110 | 8 | 40 | 35 | 1.5 | 220 | 0 |
| Crossbowman | Medium(1) | Ranged(1) | Pierce(1) | Light(0) | 30 | 22 | 55 | 180 | 2.0 | 280 | 0 |
| HeavyCavalry | Large(2) | Melee(0) | Normal(0) | Heavy(1) | 85 | 28 | 110 | 35 | 1.3 | 290 | 0 |
| LightCavalry | Medium(1) | Melee(0) | Normal(0) | Light(0) | 25 | 8 | 180 | 30 | 0.8 | 450 | 0 |

`unit_color` 使用占位色值（后续素材管线上色）。

**验收项：**
- 删除旧的 Swordsman、Archer、Cavalry、Werewolf、HeavySwordsman、WerewolfLord 记录
- 新增 10 条王国军单位记录
- `SqliteUnitRepository` 能正常加载全部 10 条

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
| barracks | 兵营 | 3 | Barracks | 28 |
| shield_camp | 盾营 | 3 | ShieldCamp | 20 |
| archery_range | 靶场 | 4 | ArcheryRange | 24 |
| stable | 马厩 | 5 | Stable | 20 |
| scout_camp | 斥候营 | 2 | ScoutCamp | 16 |

**验收项：**
- `shop_catalog` 表含 5 条王国军 T1 建筑卡，各带权重
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
- 新增 10 条王国军建筑视觉定义，footprint 均为 2×2
- 各线 Modulate 颜色：步兵线蓝灰、盾卫线深灰、射手线绿、骑兵线棕、斥候线浅蓝；T2 略亮
- 放置预览、碰撞体渲染与 2×2 占地一致

### 3.6 集成测试

验证人类种族从商店购买 → 放置 → 产兵 → 战斗的完整链路。

**验收项：**
- 商店展示 5 种 T1 建筑卡（兵营/盾营/靶场/马厩/斥候营），按权重随机
- 购买并放置兵营 → 8s 产出枪兵 → 枪兵向敌方推进
- 购买并放置盾营 → 12s 产出盾兵 → 盾兵缓慢推进，高血量低伤害
- 购买并放置靶场 → 10s 产出弓箭手 → 弓箭手远程攻击（200 范围）
- 购买并放置马厩 → 14s 产出骑士 → 骑士高速移动（120）
- 购买并放置斥候营 → 6s 产出斥候 → 斥候极速移动（160），视野 400
- 伤害倍率生效：弓箭手（Pierce）对骑士（Heavy）伤害 0.9×
- 现有战报录制/回放功能不受影响
