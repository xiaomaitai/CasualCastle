# 节点结构设计

## 原则

每个实体节点拆为三层：**Logic / View / Effects**。逻辑不碰渲染，特效不改碰撞。

```
Entity (Area2D / Node2D)
├── Logic (Node2D)       ← 逻辑层：寻路、碰撞、血量，不上屏
├── View (Node2D)      ← 表现层：精灵、动画，仅本地客户端
└── Effects (Node2D)   ← 特效层：粒子、拖尾、状态图标
```

## Unit 示例

```
Unit (Area2D)
├── Logic (Node2D)
│   └── NavigationAgent (NavigationAgent2D)
├── CollisionShape (CollisionShape2D)
└── View (Node2D)
    └── UnitCard (Node2D)          ← UnitCardView
        ├── CardBase (Sprite2D)
        ├── CardArt (Node2D)       ← CardArtView
        ├── NameLabel (Label)
        ├── StatusLabel (Label)
        ├── BuffLabel (Label)
        └── HealthBar (Node2D)     ← HealthBarView
```

外观管理（颜色、ZIndex、翻转、受击闪白）由 `UnitVisual` 类统一协调 `UnitCardView` / `CardArtView`。

**职责归属：**

| 节点 | 类型 | 职责 | 修改者 |
|------|------|------|--------|
| `Logic` | Node2D | 容纳逻辑组件 | — |
| `Logic/NavigationAgent` | NavigationAgent2D | 寻路 | UnitLogic._Process 设 TargetPosition |
| `CollisionShape` | CollisionShape2D | 碰撞体 | Unit 设半径 |
| `View` | Node2D | 表现容器 | 运动特效脚本改 Scale / Position |
| `View/UnitCard` | UnitCardView | 单位卡牌（名称、血量条、状态/Buff 标签） | UnitVisual 调 SetHealth/SetStatus/SetBuffs/SetFlipH/SetPortraitTint |
| `View/UnitCard/CardArt` | CardArtView | 兵种肖像图（动态加载、缩放填充遮罩、翻转、染色） | UnitCardView 调 SetPortrait，UnitVisual 调 SetPortraitTint/SetFlipH |

**补充（Phase 五后）：**
- `UnitLogic` 只做领域桥接与节点生命周期；表现侧拆为独立组件——`ProjectilePresenter`（弹道/素材/音效映射）、`UnitSfx`（音效事件映射）、`DeathSideEffects`（黑魔法/经验/召唤等死亡副作用）、`UnitPresentationModel`（Buff 只读展示模型）
- 单位统一生成经 `UnitSpawnService`（产兵/仪式召唤共用），`UnitLogic` 不再重复 load/初始化

**关键规则：**
- `UnitVisual` 是非节点类，通过 `UnitCardView` 和 `CardArtView` 的 API 控制所有外观
- CardArt 肖像按短边填满遮罩区域，左右移动时自动水平翻转
- 碰撞体是 `Unit` 直接子节点，任何视觉变换不影响碰撞
- 睡眠/受击状态通过 `UnitCardView` 的状态指示器（颜色圆圈 + "Z"/"!" 标签）显示

## Building 示例

同样按三层拆：

```
Building (Area2D，薄壳：装配领域桥接与能力组件)
├── Logic (Node2D)
│   ├── CollisionShape
│   └── NavigationObstacle
├── View (Node2D)
│   └── Sprite
└── Effects (Node2D)
    └── BuildingStateIcon
```

**补充（Phase 四后）：**
- `Building.cs` 只做节点生命周期与组件装配；领域状态在 `BuildingRuntime`，能力逐帧逻辑在 `IBuildingComponent` 六件套（Trap/TreasureBox/Living/Mana/Plague/CrystalMine），仪式在 `RitualHostComponent`，产兵在 `ProductionComponent`→`UnitSpawnService`
- 领域接口投影集中在 `BuildingBridge`（IBuildingState/IBuildingRef/IBuildingTarget），`Castle.GetBuildingStates()` 返回桥接对象
- 表现分离：`BuildingView` 管贴图/工作材质/状态图标/部落指示，`BuildingEffects` 管工作跳动/受损闪色/摧毁粒子

## 特效组件约定

附着在 View 节点上的运动特效遵循：
- 继承 `Node2D`，挂载在 View 节点自身
- 通过追踪父节点（实体）的 `GlobalPosition` 差判断运动状态
- 只修改自身（View）的 `Scale` 和 `Position`，不访问兄弟或子节点
- 静止时 lerp 回单位 Scale 和零 Position
