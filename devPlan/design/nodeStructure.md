# 节点结构设计

## 原则

每个实体节点拆为三层：**Logic / View / Effects**。逻辑不碰渲染，特效不改碰撞。

```
Entity (Area2D / Node2D)
├── Logic (Node2D)       ← 逻辑层：寻路、碰撞、血量，不上屏
├── View (Node2D)      ← 表现层：精灵、动画，仅本地客户端
└── Effects (Node2D)   ← 特效层：粒子、拖尾、状态图标
```

## Soldier 示例

```
Soldier (Area2D)
├── Logic (Node2D)
│   ├── NavigationAgent
│   └── CollisionShape
├── View (Node2D)          ← 可挂运动动画脚本（伸缩、弹跳等），只改自身 transform
│   └── Sprite
└── Effects (Node2D)
    └── SleepZEffect
```

**职责归属：**

| 节点 | 类型 | 职责 | 修改者 |
|------|------|------|--------|
| `Logic` | Node2D | 容纳逻辑组件（需 Node2D 以支持 NavigationAgent 等 2D 节点） | — |
| `Logic/NavigationAgent` | NavigationAgent2D | 寻路 | Soldier._Process 设 TargetPosition |
| `Logic/CollisionShape` | CollisionShape2D | 碰撞体 | Soldier.ApplyPendingStats 设半径 |
| `View` | Node2D | 表现容器，承载运动动画 | 运动特效脚本改 Scale / Position |
| `View/Sprite` | Sprite2D | 基础显示 | Soldier.ApplyPendingStats 设 Scale / Position |
| `Effects` | Node2D | 特效容器 | 各特效组件 |
| `Effects/SleepZEffect` | Node2D | 休眠 Z 标记 | SoldierSleepZEffect |

**关键规则：**
- View 层动画脚本（如 squash & stretch）只改 View 节点的 Scale / Position，不碰 Sprite
- Sprite 的基础 Scale / Position 由 Soldier 代码设置，View 的 transform 叠加其上
- CollisionShape 在 Logic 下，任何视觉变换不影响碰撞
- Effects 下的所有特效不参与逻辑判定

## Building 示例

同样按三层拆：

```
Building (Area2D)
├── Logic (Node2D)
│   ├── CollisionShape
│   └── NavigationObstacle
├── View (Node2D)
│   └── Sprite
└── Effects (Node2D)
    └── BuildingStateIcon
```

## 特效组件约定

附着在 View 节点上的运动特效遵循：
- 继承 `Node2D`，挂载在 View 节点自身
- 通过追踪父节点（实体）的 `GlobalPosition` 差判断运动状态
- 只修改自身（View）的 `Scale` 和 `Position`，不访问兄弟或子节点
- 静止时 lerp 回单位 Scale 和零 Position
