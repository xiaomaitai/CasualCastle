---
name: git-commit-powershell
description: >-
  在 Windows PowerShell 下执行 git 提交。使用 git commit -m 单行消息，禁止 heredoc
  与 @ 字符串语法，避免命令被 allowlist 拦截。用户要求提交时直接执行，不要额外确认。
---

# Git Commit（Windows / PowerShell）

## 必须遵守

1. **只用** `git commit -m "提交说明"`（单行双引号字符串）
2. **禁止** bash heredoc：`$(cat <<'EOF' ... EOF)`
3. **禁止** PowerShell here-string：`$( @' ... '@ )` 或 `@'...'@`
4. **禁止** `&&` 链接命令，用 `;` 或分步执行
5. 用户已明确要求提交时，**直接执行**，不要因语法问题反复征求确认

## 原因

`git commit -m "$( @'...'@ )"` 等写法常被判定为不在 allowlist 内，从而弹出额外审批。单行 `-m` 可正常通过。

## 标准流程

```powershell
git status --short
git diff --stat
git log --oneline -3
```

```powershell
git add path/to/file1 path/to/file2
```

```powershell
git commit -m "Add castle grid buildings and passable empty cells"
```

需要 push 时用户会单独说明；默认 **不 push**。

## 提交说明

- 一行英文或中文均可，简洁说明「为什么」
- 较长内容压缩为一行，用逗号或 and 连接，**不要**拆成多行 heredoc

## 示例

```powershell
# 正确
git commit -m "Fix player soldiers stopping at misaligned castle collision"

# 错误 — 会触发 allowlist 审批
git commit -m "$( @'
Fix bug
'@ )"

# 错误 — PowerShell 不支持
git add . && git commit -m "msg"
```
