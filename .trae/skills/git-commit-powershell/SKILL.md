---
name: "git-commit-powershell"
description: "Executes git commits correctly on Windows PowerShell. Invoke when user asks to commit changes in a Windows/PowerShell environment."
---

# Git Commit (Windows/PowerShell)

Windows PowerShell 环境下执行 git 提交的正确流程。

## 关键规则

### 1. 不要用 `&&`
PowerShell **不支持** `&&` 作为命令分隔符。要用 `;` 或者分步执行。

### 2. 不要用 bash heredoc
PowerShell **不支持** bash heredoc 语法（`$(cat <<'EOF' ...)`）。
提交信息直接用单行引号或双引号字符串。

### 3. 正确做法

```powershell
# ❌ 错误 - PowerShell 不支持 && 
git add . && git commit -m "message"

# ✅ 正确 - 分步执行
git add file1.cs file2.cs
git commit -m "message"

# ✅ 正确 - 或用 ; 分隔
git add file1.cs; git commit -m "message"
```

## 标准提交流程

### Step 1: 查看状态
```powershell
git status
git diff --stat
git log --oneline -3
```

### Step 2: Stage 文件
```powershell
git add <file1> <file2> ...
```
不要在 `git add` 里用 `-A` 或 `.`，指定具体文件。

### Step 3: 提交
```powershell
git commit -m "commit message here"
```
用 `-m` 参数 + 双引号字符串即可，不要用 heredoc。

## 说明
这个 skill 只处理 git 提交相关的 PowerShell 兼容性问题。其他 git 操作（push、pull、rebase 等）不在其范围。