---
name: "git-commit-powershell"
description: "Executes git commits correctly on Windows PowerShell. Invoke when user asks to commit changes in a Windows/PowerShell environment."
---

# Git Commit (Windows/PowerShell)

> 主副本：`.cursor/skills/git-commit-powershell/SKILL.md`（Cursor Agent 优先读取）

Windows PowerShell 环境下执行 git 提交的正确流程。

## 关键规则

### 1. 不要用 `&&`
PowerShell **不支持** `&&` 作为命令分隔符。要用 `;` 或者分步执行。

### 2. 不要用 heredoc / here-string
- **禁止** bash heredoc：`$(cat <<'EOF' ... EOF)`
- **禁止** PowerShell：`$( @'...'@ )`
- 这类写法易被 allowlist 拦截，导致每次都要用户确认

### 3. 正确做法

```powershell
git add file1.cs file2.cs
git commit -m "commit message here"
```

用户已要求提交时，直接执行，不要额外确认。

## 标准提交流程

1. `git status` / `git diff --stat` / `git log --oneline -3`
2. `git add <具体文件>`
3. `git commit -m "单行提交说明"`

不要用 `git add -A` 或 `.`，指定具体文件。
