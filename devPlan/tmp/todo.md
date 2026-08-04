# 架构待办（TODO）

## 冒险者公会（联盟派系流派 2/3）

多种族佣兵联盟，公会产人 + 酒馆组队。设计见 `devPlan/design/factions/allianceFaction.md` 第 4 章。

- 酒馆组队逻辑：公会产出的人前往最近酒馆 → 凑齐 N 个不同种族 → 全队出发
- T1 酒馆 N=4，T2 公会 N=6
- 数据层已落地：4 座公会建筑 + 酒馆 + 8 种兵种、T1→T2 配方、商店卡池、`unit_stats.race` 字段、鱼龙混杂闪避（skill_defs + 技能系统）
