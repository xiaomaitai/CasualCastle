# 架构待办（TODO）

## 冒险者公会（联盟派系流派 2/3）

多种族佣兵联盟，公会产人 + 酒馆组队。设计见 `devPlan/design/factions/allianceFaction.md` 第 4 章。

- 酒馆组队逻辑：公会产出的人前往最近酒馆 → 凑齐 N 个不同种族 → 全队出发
- T1 酒馆 N=4，T2 公会 N=6
- 数据层已落地：4 座公会建筑 + 酒馆 + 8 种兵种、T1→T2 配方、商店卡池、`unit_stats.race` 字段、鱼龙混杂闪避（skill_defs + 技能系统）

## 神殿卫士（联盟派系流派 3/3）

治疗续航型流派。设计见 `devPlan/design/factions/allianceFaction.md` 第 5 章。

- 3 座 T1：圣所、圣泉、裁判所
- 3 座 T2：圣殿、圣光圣所、裁决厅
- 6 种兵种：圣盾兵→圣骑士、牧师→大主教、审判官→惩戒者
- 新系统：治疗单位 AI（牧师不攻击，改为治疗周围友军）
- 核心机制：恩典——受到治疗触发各兵种特有额外效果（护盾/冷却缩减/范围伤害）
- 商店卡池新增 3 种卡

## 商店卡池数据重复

`shop_catalog` 存在新旧两套 ID 命名：王国军 5 张 T1 卡同时存在小写旧条目（如 `barracks`）与 `tech_tree_nodes.type_id` 同步的新条目（如 `Barracks`），公会 5 卡只有旧 `card_*` 条目。需以 `tech_tree_nodes.type_id` 为准清理旧行——`SqliteTechTreeRepository.SyncToGameTables` 的删除逻辑只清理 `shop_available = 0` 且能关联到节点的行，无节点对应的旧条目会残留。
