# v0.7 完善版 — G3 可玩性打磨 + 操作 QOL（当前任务）

> 版本规划见 `devPlan/outline/c05Milestones.md`。本文件细化当前阶段 **G3**。
> 前置：G1 ✅ G2 ✅（G2 完成：联盟 7 兵种 + 死亡天使技能化，Domain 625 / Persistence 30 / Application 24 全绿）。

## 范围拆分

### G3-A 基础打磨
- [x] A1 商店夜晚解锁：`unlock_night` 规则 + 领域规则 + 配置 + 测试（dd682a9）
- [x] A3 补 alliance / horde battle 冒烟（AllianceBattleSmoke + HordeBattleSmoke，5020f6d）
- [ ] A2 属性面板数据缺口：§8 的 7 项缺口已细分记入 `devPlan/tmp/todo.md`，多为「机制缺失非展示缺失」——待对应机制（抗性字段/攻击范围/击杀奖励/buff 只读端口/建筑攻防/研发成长）落地后补齐展示；buff 只读端口建议随 G4「buff 栏」一并做
- [ ] A0 数值调优：四派系占位数值实测调优需实机对局数据支撑，作为持续项跟进

### G3-B 操作 QOL
- [x] B1 加速面板开放：常显 + 1/2/3/4× 档位 + 设置记忆（edc37bd）
- [x] B2 自动暂停设置：夜晚自动暂停 / 放置后自动继续（edc37bd）
- [x] B3 快捷键：数字 1-9 选卡（商店开时转快速购买）、空格暂停、B 商店、S 卖出选中、M 静音、Esc 逐级关闭、右键取消（edc37bd）
- [x] B4 镜头操作：Home 定位己方核心 + 核心受袭红边告警（f44fb91）；交战带快捷定位未做（G4 范围预览一并）
- [x] B5 结算「再来一局」+ 商店快速购买（数字键）（edc37bd）

## 验收标准
- [x] 每项 QOL 均可在局内完成并生效
- [x] 派系 battle 冒烟新增并编译通过
- [x] Domain 627 / Persistence 30 / Application 24 全绿、dotnet build 零 warning
- [ ] 属性面板补齐项逐一可读（见 todo：机制到位后补）

## 下一步
G3 主体完成。A2/A0 处置见上。下一阶段为 **G4 UI 与交互 + 交互/展示 QOL**（`c05Milestones.md`）——待用户细化。
