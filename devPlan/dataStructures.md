# 数据结构设计 — CasualCastle

卡牌、建筑、部队等核心运行时/配置数据的 C# 结构草案。概念定义见 `concepts.md`，开发顺序与系统划分见 `outline/`。

---

## 卡牌数据

```csharp
public class CardData
{
	public string Id;           // 卡牌ID
	public string Name;         // 卡牌名称
	public int Cost;           // 购买费用
	public string BuildingType; // 对应建筑类型
	public Texture2D Icon;     // 卡牌图标
}
```

---

## 建筑数据

```csharp
public class BuildingData
{
	public string Id;           // 建筑ID
	public string Name;         // 建筑名称
	public int Health;          // 生命值
	public float ProductionInterval; // 产出间隔
	public string UnitType;     // 产出部队类型
	public List<string> AdjacentBonuses; // 邻接加成列表
	public int FusionLevel;     // 融合等级
}
```

---

## 部队数据

```csharp
public class UnitData
{
	public string Id;           // 部队ID
	public string Name;         // 部队名称
	public int Damage;          // 攻击力
	public int Health;          // 生命值
	public float Speed;         // 移动速度
	public bool NightActive;    // 是否夜间活动
	public float VisionRange;   // 视野范围
}
```
