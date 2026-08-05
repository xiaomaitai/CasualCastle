# 数据结构设计 — CasualCastle

卡牌、建筑、单位等核心运行时/配置数据的 C# 结构。概念定义见 `concepts.md`，开发顺序与系统划分见 `outline/`。

> 本文档为早期设计参考。所有实际游戏数据以 `assets/data/config.db` 为准；文档与数据库不一致时以数据库为准。

---

## 卡牌数据

```csharp
public class CardData
{
	public string Id;           // 卡牌 ID，与建筑 type_id 一致
	public string Name;         // 卡牌名称
	public int Cost;            // 购买费用
	public string BuildingType; // 对应建筑类型
	public int Weight;          // 卡池权重
}
```

商店卡池 `shop_catalog` 以 `tech_tree_nodes.type_id` 为权威 ID；建筑卡 ID 与建筑 `type_id` 一致，同一建筑不允许出现多条卡池记录。

---

## 建筑数据

```csharp
public class BuildingData
{
	public string TypeId;               // 建筑类型 ID
	public GridCellOffset[] Footprint;  // 占地格偏移
	public GridCellOffset MainCellOffset;  // 主格偏移（邻接/图标定位）
	public string DisplayName;          // 显示名
	public int MaxHealth;               // 最大生命
	public float SpawnInterval;         // 旧产兵间隔字段，编辑器维护，不驱动产兵
	public float ProductionRate;        // 产兵生产率（点数/秒，统一 100）
	public GridCellOffset SpawnCellOffset;  // 产兵点偏移
	public string UnitTypeId;           // 产出单位类型
	public bool IsCore;                 // 核心建筑（城堡之心）
	public int CombineTier;             // 组合阶数（0 = 未组合）
	public int CollisionWidth;          // 碰撞尺寸（游戏单位）
	public int CollisionHeight;
}
```

产兵采用**点数累积制**：工作进度按 `ProductionRate` 每秒累计，累计满产出单位（`UnitTypeId`）的 `UnitCost` 后产兵并清零；耗时 = `UnitCost` / `ProductionRate` 秒。规则见 `design/battleSystem.md` §2.2。

---

## 组合配方

```csharp
public class CombineRecipe
{
	public string MainTypeId;      // 主体建筑类型（结果落点）
	public string MaterialTypeId;  // 辅材建筑类型
	public int MaterialCount;      // 须与主体邻接的辅材数量
	public string ResultTypeId;    // 结果建筑类型
}
```

配方存于 `config.db` 的 `combine_recipes`，由科技树编辑器维护。

---

## 战报

```csharp
public class BuildingSnapshot
{
	public string TypeId;
	public int AnchorGridX;
	public int AnchorGridY;
	public int Health;
	public bool IsManuallyPaused;
	public bool IsCombineProhibited;
}

public enum BattlePhase { Day, Night, Combine }

public class CardSnapshot
{
	public string BuildingType;         // 手牌对应建筑类型
	public int Cost;                    // 购买价格（= 建筑价值）
}

public class CastleSnapshot
{
	public int NightIndex;              // 第几个昼夜循环
	public BattlePhase Phase;           // 阶段：白天 / 夜晚 / 合成
	public List<BuildingSnapshot> Buildings;
	public int Gold;                    // 快照时玩家金币（存款）
	public List<CardSnapshot> HandCards; // 快照时玩家手牌
}

public class BattleReport
{
	public string ReportId;
	public string DisplayName;          // 可选，默认时间戳
	public long SavedAtUnix;            // 保存时间
	public List<CastleSnapshot> Nights;
}
```

局内缓存可用 `BattleReport` 不含 `ReportId`；持久化时分配 ID 并写入磁盘。

---

## 单位数据

```csharp
public class UnitStats
{
	public string TypeId;        // 单位类型 ID
	public UnitSize Size;        // 体型，决定碰撞半径与显示尺寸
	public AttackType AttackType;   // 近战/远程
	public DamageType DamageType;   // 物理/穿刺/魔法等
	public ArmorType ArmorType;     // 护甲类型
	public int ArmorValue;       // 护甲值
	public int Health;           // 生命值
	public int Damage;           // 攻击力
	public float Speed;          // 移动速度
	public float AttackRange;    // 攻击范围
	public float AttackCooldown; // 攻击冷却
	public float VisionRange;    // 视野范围
	public int UnitCost;         // 产兵所需点数
	public string Race;          // 种族（鱼龙混杂闪避判定）
}
```

`UnitCost` 必须 > 0；为 0 视为数据未配置，对应建筑不启动产兵。产兵耗时 = `UnitCost` / 100 秒。

---

## 科技树节点

```csharp
public class TechTreeNode
{
	public string TypeId;      // 与 building_defs.type_id 一致
	public string FactionId;   // 派系 ID
	public int Tier;           // 阶数 1~5
	public int Col;            // 画布列位置
	public bool ShopAvailable; // 是否进入商店卡池
	public int GoldCost;       // 商店价格
	public int ShopWeight;     // 卡池权重
	public string DisplayName; // 显示名
	public string UnitTypeId;  // 产出单位类型
	public int MaxHealth;      // 建筑最大生命
	public float SpawnInterval; // 旧产兵间隔字段
}
```

科技树编辑器保存时以本表为准重建该派系的 `building_defs` 与 `shop_catalog`：建筑定义保留 `production_rate`，商店卡 ID = 建筑 `type_id`。
