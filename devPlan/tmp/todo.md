# 架构待办（TODO）

## 建筑素材图集化（素材量增大后）

同屏数十座建筑时，单张 `_game.png` 纹理的绑定/批处理收益有限，暂不改造；等建筑素材量或同屏规模进一步增大后，改用 Godot 图集导入（`ResourceImporterTextureAtlas`）。

- 前置：保持 `_cut → _game` 阶段不变，图集只打包裁边 + 512/256 的 `_game.png`
- 按阵营或阶级拆 2~3 张图集（单张 4096 上限装不下全部 51 座）
- DB 引用 Godot 生成的图集区域资源，重打包自动跟随，不写死坐标
- 开启 `trim_alpha_border_from_region`；注意图集区域与 mipmap 冲突（当前已关 mipmap）
- 先试点（如联盟建筑）验证导入流程与批处理收益，再铺开