# 开发大纲 — CasualCastle（中文）

## 一、概述

这是一个简单的2D RTS游戏，类似皇室战争的玩法。

**核心玩法循环：**
1. **夜晚阶段**：玩家在商店购买建筑卡牌，卡牌进入手牌
2. **建造阶段**：玩家在自己的城堡区域打出建筑
3. **邻接加成**：建筑之间产生特殊的邻接加成效果
4. **融合升级**：在下一个夜晚来临时可融合和升级建筑（类似背包乱斗）
5. **战斗阶段**：出兵建筑周期性产生部队，在战场中央与敌方部队交战
6. **胜利条件**：摧毁对方城堡获得胜利

**夜间机制**：
- 普通部队只会攻击视野范围内的敌人，否则原地睡觉
- 特殊部队（狼人、刺客）可以在夜间继续行动

---

## 二、MVP（最小可行产品）

### 2.0 极简 MVP（✅ 已完成，2026-06-15）

最小可玩循环：**兵营产兵 → 士兵前进/战斗 → 攻城堡 → 胜负判定 → 重开**。

#### 核心目标

| 目标 | 状态 |
|------|------|
| 双方各有一座兵营 | ✅ |
| 兵营自动产出士兵 | ✅ |
| 单位自动移动并战斗 | ✅ |
| 城堡血量与胜负判定 | ✅ |
| 完整游戏循环人工验证 | ✅ |

#### 已完成模块

| 模块 | 交付内容 | 关键文件 |
|------|----------|----------|
| 基础框架 | 主场景、窗口 1280×720、`GameManager` / `UIManager` | `scenes/main/main_game.tscn`, `scripts/autoload/` |
| 兵营与单位 | 兵营计时产兵、士兵属性与预制体 | `prefabs/Barracks.tscn`, `prefabs/Soldier.tscn`, `scripts/nodes/Barracks.cs`, `scripts/nodes/Soldier.cs` |
| 战斗系统 | 移动、互殴、攻建筑、死亡 | `scripts/nodes/Soldier.cs` |
| 城堡与胜负 | 城堡预制体、扣血、结算、重开 | `prefabs/Castle.tscn`, `scripts/nodes/Castle.cs` |
| 场景配置 | 格子放置兵营、碰撞层、战场布局 | `scripts/nodes/Building.cs`, `scripts/nodes/Castle.cs` |

#### 阶段交付摘要

1. **基础框架** — 主游戏场景、项目配置、管理器节点
2. **兵营与单位** — `Barracks` / `Soldier` 预制体与自动产兵
3. **战斗系统** — 追击、攻击冷却、单位死亡；士兵可穿过城堡空闲格
4. **城堡与胜负** — `Castle` 预制体、士兵经建筑碰撞攻城堡、`GameManager` 胜负与 UI 重开
5. **场景配置** — 城堡格子分配兵营（玩家 7,4 / 敌方 0,4），建筑碰撞 56×56，产兵于相邻格左下角

概念与实现细节见 `dev_plan/concepts.md`。

---

### 2.1 完整版功能清单（待开发）

| 模块 | 功能 | 状态 |
|------|------|------|
| 游戏流程 | 昼夜交替循环 | 待开发 |
| 商店系统 | 购买建筑卡牌 | 待开发 |
| 手牌系统 | 卡牌管理、打出 | 待开发 |
| 建筑系统 | 建筑放置、属性、产出 | 待开发 |
| 战斗系统 | 部队AI、攻击、死亡 | 部分完成（极简战斗已实现） |
| 邻接系统 | 建筑邻接加成计算 | 待开发 |
| 融合系统 | 建筑融合升级 | 待开发 |
| UI界面 | 主菜单、游戏HUD、结算界面 | 部分完成（对战 HUD / 结算已实现） |

### 2.2 完整版目标

- 支持单人对战AI模式
- 至少5种基础建筑类型
- 完整的昼夜循环机制
- 简单的战斗AI
- 基础的邻接加成系统

---

## 三、项目架构

### 3.1 目录结构

```
CasualCastle/
├── scripts/                    # C#脚本目录
│   ├── autoload/              # 全局单例（自动加载）
│   │   ├── GameManager.cs     # 游戏主管理
│   │   ├── CardSystem.cs      # 卡牌系统
│   │   ├── BattleSystem.cs    # 战斗系统
│   │   └── UISystem.cs        # UI系统
│   ├── systems/               # 游戏系统
│   │   ├── ShopSystem.cs      # 商店系统
│   │   ├── BuildingSystem.cs  # 建筑系统
│   │   ├── AdjacentSystem.cs  # 邻接加成系统
│   │   ├── FusionSystem.cs    # 融合升级系统
│   │   └── NightSystem.cs     # 夜间机制系统
│   ├── utils/                 # 工具类
│   │   ├── GameConfig.cs      # 游戏配置
│   │   ├── Pathfinding.cs     # 寻路算法
│   │   └── MathUtils.cs       # 数学工具
│   └── nodes/                 # 场景节点脚本
│       ├── Building.cs        # 单格建筑基类
│       ├── Barracks.cs        # 兵营
│       ├── Soldier.cs         # 士兵
│       └── Castle.cs          # 城堡
├── scenes/                    # 场景文件
│   └── main/
│       └── main_game.tscn     # 主游戏场景（极简 MVP 入口）
├── prefabs/                   # 预制件
│   ├── Castle.tscn
│   ├── Barracks.tscn
│   └── Soldier.tscn
└── assets/                    # 游戏资源
    ├── art/                   # 美术资源
    ├── audio/                 # 音效音乐
    └── fonts/                 # 字体
```

### 3.2 核心系统说明

#### 3.2.1 GameManager（游戏管理器）
- 管理游戏状态（菜单/游戏中/结算）
- 控制昼夜循环切换
- 管理玩家和AI状态

#### 3.2.2 CardSystem（卡牌系统）
- 手牌管理（添加、移除、排序）
- 卡牌打出逻辑
- 卡牌数据加载

#### 3.2.3 ShopSystem（商店系统）
- 商店物品刷新
- 购买逻辑
- 金币管理

#### 3.2.4 BuildingSystem（建筑系统）
- 建筑放置验证
- 建筑属性管理
- 建筑产出逻辑

#### 3.2.5 BattleSystem（战斗系统）
- 部队生成与管理
- 战斗AI
- 伤害计算

#### 3.2.6 AdjacentSystem（邻接系统）
- 邻接检测
- 加成效果计算
- 实时更新加成

#### 3.2.7 FusionSystem（融合系统）
- 融合条件检测
- 升级逻辑
- 新建筑生成

---

## 四、开发顺序

### 4.1 第一阶段：基础框架（1-2周）

| 任务 | 描述 | 预估时间 |
|------|------|----------|
| 项目初始化 | Godot项目配置、目录结构搭建 | 1天 |
| GameManager | 游戏状态管理、昼夜循环框架 | 2天 |
| UI框架 | 主菜单、游戏HUD基础 | 2天 |
| 资源配置 | 卡牌、建筑、部队数据结构设计 | 2天 |

**交付物：**
- 可运行的空项目框架
- 游戏状态切换功能
- 基础UI界面

### 4.2 第二阶段：商店与手牌（1周）

| 任务 | 描述 | 预估时间 |
|------|------|----------|
| ShopSystem | 商店刷新、购买逻辑 | 2天 |
| CardSystem | 手牌管理、打出逻辑 | 2天 |
| 卡牌UI | 卡牌显示、拖拽交互 | 2天 |

**交付物：**
- 商店购买功能
- 手牌系统
- 卡牌拖拽打出

### 4.3 第三阶段：建筑系统（1-2周）

| 任务 | 描述 | 预估时间 |
|------|------|----------|
| BuildingSystem | 建筑放置、属性管理 | 3天 |
| 建筑类型 | 5种基础建筑实现 | 3天 |
| AdjacentSystem | 邻接检测与加成计算 | 2天 |

**交付物：**
- 建筑放置系统
- 建筑产出功能
- 邻接加成效果

### 4.4 第四阶段：战斗系统（2周）

| 任务 | 描述 | 预估时间 |
|------|------|----------|
| Unit组件 | 部队属性、移动、攻击 | 3天 |
| BattleSystem | 战斗AI、碰撞检测 | 4天 |
| NightSystem | 夜间机制、睡眠/警戒状态 | 2天 |

**交付物：**
- 部队生成与控制
- 战斗AI逻辑
- 夜间机制

### 4.5 第五阶段：融合系统（1周）

| 任务 | 描述 | 预估时间 |
|------|------|----------|
| FusionSystem | 融合条件、升级逻辑 | 3天 |
| 升级UI | 融合界面、选择交互 | 2天 |

**交付物：**
- 建筑融合升级功能
- 融合界面

### 4.6 第六阶段：AI与优化（1周）

| 任务 | 描述 | 预估时间 |
|------|------|----------|
| AI对手 | 简单AI决策逻辑 | 3天 |
| 性能优化 | 战斗性能、渲染优化 | 2天 |
| Bug修复 | 测试与问题修复 | 2天 |

**交付物：**
- AI对战功能
- 性能优化完成

---

## 五、数据结构设计

### 5.1 卡牌数据

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

### 5.2 建筑数据

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

### 5.3 部队数据

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

---

## 六、里程碑

| 里程碑 | 目标 | 状态 |
|--------|------|------|
| M0 | 极简 MVP：产兵对战与胜负循环 | ✅ 已完成 |
| M1 | 基础框架完成，可进入游戏 | 待开发 |
| M2 | 商店与手牌系统完成 | 待开发 |
| M3 | 建筑与邻接系统完成 | 待开发 |
| M4 | 战斗系统完成 | 待开发 |
| M5 | 融合系统完成 | 待开发 |
| M6 | 完整版 MVP 发布 | 待开发 |

---

## 七、技术栈

| 分类 | 技术 |
|------|------|
| 引擎 | Godot 4.6 (C#) |
| 渲染 | Forward+ |
| 物理 | Jolt Physics |
| 语言 | C# / .NET |
