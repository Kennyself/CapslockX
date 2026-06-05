# CapslockX 需求规格说明书

> **版本**: v1.0.0  
> **创建日期**: 2026-06-03  
> **最后更新**: 2026-06-04  
> **状态**: Phase 1 MVP 完成 ✅  

---

## 目录

1. [产品概述](#1-产品概述)
2. [核心理念](#2-核心理念)
3. [功能需求](#3-功能需求)
4. [非功能需求](#4-非功能需求)
5. [技术约束](#5-技术约束)
6. [配置结构](#6-配置结构)
7. [开发计划](#7-开发计划)
8. [待决策事项](#8-待决策事项)
9. [变更记录](#9-变更记录)

---

## 1. 产品概述

### 1.1 产品定位

将 Windows 上极少使用的 **CapsLock 键**改造为强大的**修饰键**，通过组合键实现光标移动、文本编辑、窗口管理等高频操作，让用户无需离开主键区（Home Row）即可完成大部分日常操作。

### 1.2 目标平台

- **主平台**: Windows 11（x64）
- **兼容性**: Windows 10 x64

### 1.3 目标用户

- 经常进行文本编辑的程序员、写作者
- 追求键盘操作效率的高级用户
- 希望减少鼠标依赖、降低手腕疲劳的用户

### 1.4 参考产品

| 产品 | 平台 | 说明 |
|------|------|------|
| [Capslox](https://capslox.com) | macOS / Windows | 商业产品，三大修饰键体系 |
| [Capslock+](https://github.com/wo52616111/capslock-plus) | Windows | 开源工具，功能全面 |

---

## 2. 核心理念

### 2.1 CapsLock 行为模型（已实现）

| 场景 | 用户操作 | CapsLock 切换 | 其他按键 | 实现方式 |
|------|---------|:---:|:---:|------|
| **短按** | 按下+松开 < 300ms，无其他按键 | ✅ **切换** | 正常输入 | CapsLock DOWN/UP 均拦截；SendInput 模拟一次 CapsLock 切换 |
| **长按（无组合键）** | 按下 > 300ms 后松开，无其他按键 | ❌ **不切换** | 正常输入 | CapsLock DOWN/UP 均拦截，不发送任何切换 |
| **组合键** | 按住 CapsLock + 按下其他键 | ❌ **不切换** | 拦截+执行绑定 | CapsLock DOWN/UP 均拦截；其他键拦截并转译为对应快捷键 |

### 2.2 设计原则

1. **短按行为不变** — 短按 CapsLock 必须切换大小写，不能有任何差异
2. **长按无响应** — 长按 CapsLock 不切换大小写，不产生任何副作用
3. **手指不离开主键区** — 方向键、功能键均在主键区范围内
4. **零感知延迟** — 键盘钩子处理必须在 10ms 内完成
5. **可定制化** — 所有快捷键均可重新绑定
6. **开箱即用** — 默认配置满足 80% 场景

### 2.3 阈值参数

| 参数 | 默认值 | 说明 |
|------|--------|------|
| `longPressThresholdMs` | 300ms | 区分短按（原生行为）和长按（修饰键模式）的时长阈值 |

---

## 3. 功能需求

### 3.1 模块总览

```
CapslockX
├── 3.2 光标移动（Cursor Navigation）
├── 3.3 文本选择与编辑（Text Selection & Editing）
├── 3.4 窗口管理（Window Management）
├── 3.5 Tab / 标签页管理（Tab Management）
├── 3.6 功能键模拟（Function Key Simulation）
├── 3.7 快捷功能（Quick Actions）
├── 3.8 系统托盘（System Tray）
└── 3.9 设置界面（Settings UI）
```

### 3.2 光标移动（Cursor Navigation）

> 在不离开主键区的情况下完成所有方向移动操作。✅ Phase 1 全部实现（P0）。

| ID | 快捷键 | 功能 | 优先级 | 状态 |
|----|--------|------|--------|------|
| CUR-01 | `CapsLock + E` | 光标上移（↑） | P0 | ✅ |
| CUR-02 | `CapsLock + D` | 光标下移（↓） | P0 | ✅ |
| CUR-03 | `CapsLock + S` | 光标左移（←） | P0 | ✅ |
| CUR-04 | `CapsLock + F` | 光标右移（→） | P0 | ✅ |
| CUR-05 | `CapsLock + A` | Home（跳转行首） | P0 | ✅ |
| CUR-06 | `CapsLock + G` | End（跳转行尾） | P0 | ✅ |
| CUR-07 | `CapsLock + W` | 按词向左跳转（Ctrl+←） | P1 |
| CUR-08 | `CapsLock + R` | 按词向右跳转（Ctrl+→） | P1 |
| CUR-09 | `CapsLock + Q` | Page Up（上翻页） | P1 |
| CUR-10 | `CapsLock + C` | Page Down（下翻页） | P1 |
| CUR-11 | `CapsLock + I` | 跳转到行首（同 A 键，备选映射） | P2 |
| CUR-12 | `CapsLock + K` | 跳转到行尾（同 G 键，备选映射） | P2 |

### 3.3 文本选择与编辑（Text Selection & Editing）

| ID | 快捷键 | 功能 | 优先级 | 状态 |
|----|--------|------|--------|------|
| TXT-01 | `CapsLock + Shift + (E/D/S/F)` | 选中文本（配合方向键） | P0 | ✅ |
| TXT-02 | `CapsLock + Shift + (A/G)` | 选中到行首/行尾（Shift+Home/End） | P0 | ✅ |
| TXT-03 | `CapsLock + X` | 剪切（Ctrl+X） | P0 |
| TXT-04 | `CapsLock + C` | 复制（Ctrl+C）[冲突：CUR-10 默认禁用] | P0 |
| TXT-05 | `CapsLock + V` | 粘贴（Ctrl+V） | P0 |
| TXT-06 | `CapsLock + Z` | 撤销（Ctrl+Z） | P1 |
| TXT-07 | `CapsLock + Y` | 重做（Ctrl+Y） | P1 |
| TXT-08 | `CapsLock + Backspace` | 删除前一个词（Ctrl+Backspace） | P1 |
| TXT-09 | `CapsLock + Delete` | 删除后一个词（Ctrl+Delete） | P1 |
| TXT-10 | `CapsLock + P` | 删除当前行 | P1 |
| TXT-11 | `CapsLock + Space` | 选中当前单词 / 取消选中 | P2 |
| TXT-12 | `CapsLock + ;` | 进入文本选择模式（后续方向键选中文本） | P2 |

> ⚠️ **冲突处理策略（D6 已决议）**: 当多个功能绑定同一按键时，默认仅保留一个启用，其余设为 `disabled: true`。用户可在设置中手动切换启用的功能。冲突清单：
> - `CapsLock+C`: 复制（TXT-04）← 默认启用 / Page Down（CUR-10）← 默认禁用
> - `CapsLock+W`: 按词左跳（CUR-07）← 默认启用 / 关闭标签页（TAB-05）← 默认禁用
> - `CapsLock+T`: 翻译（ACT-02）← 默认启用 / 新建标签页（TAB-04）← 默认禁用

### 3.4 窗口管理（Window Management）

| ID | 快捷键 | 功能 | 优先级 |
|----|--------|------|--------|
| WIN-01 | `CapsLock + ←` | 窗口贴左半屏 | P0 |
| WIN-02 | `CapsLock + →` | 窗口贴右半屏 | P0 |
| WIN-03 | `CapsLock + ↑` | 窗口最大化 | P0 |
| WIN-04 | `CapsLock + ↓` | 窗口还原 / 最小化 | P0 |
| WIN-05 | `CapsLock + Enter` | 窗口最大化/还原切换 | P0 |
| WIN-06 | `CapsLock + M` | 窗口最小化 | P1 |
| WIN-07 | `CapsLock + /` | 窗口居中显示 | P1 |
| WIN-08 | `CapsLock + Shift + ←/→` | 窗口移动到左侧/右侧显示器 | P1 |
| WIN-09 | `CapsLock + 1~9` | 窗口定位到屏幕九宫格区域 | P2 |
| WIN-10 | `CapsLock + Tab` | 应用窗口切换器（Alt+Tab） | P1 |
| WIN-11 | `CapsLock + `` ` `` ` | 当前窗口置顶/取消置顶 | P2 |
| WIN-12 | `CapsLock + Shift + `` ` `` ` | 切换当前窗口透明度 | P2 |

### 3.5 Tab / 标签页管理（Tab Management）

| ID | 快捷键 | 功能 | 优先级 |
|----|--------|------|--------|
| TAB-01 | `CapsLock + J` | 切换到上一个标签页（Ctrl+Shift+Tab） | P1 |
| TAB-02 | `CapsLock + L` | 切换到下一个标签页（Ctrl+Tab） | P1 |
| TAB-03 | `CapsLock + 1~9` | 跳转到第 N 个标签页（Ctrl+N） | P2 |
| TAB-04 | `CapsLock + T` | 新建标签页（Ctrl+T） | P2 |
| TAB-05 | `CapsLock + W` | 关闭当前标签页（Ctrl+W）[冲突：CUR-07 默认启用，此项默认禁用] | P2 |

> ⚠️ **冲突处理策略（D6 已决议）**: 冲突项默认禁用，冲突清单见 [3.3 节](#33-文本选择与编辑text-selection--editing)。

### 3.6 功能键模拟（Function Key Simulation）

| ID | 快捷键 | 功能 | 优先级 |
|----|--------|------|--------|
| FKEY-01 | `CapsLock + 1` | 模拟 F1 | P1 |
| FKEY-02 | `CapsLock + 2` | 模拟 F2 | P1 |
| FKEY-03 | `CapsLock + 3` | 模拟 F3 | P1 |
| FKEY-04 | `CapsLock + 4` | 模拟 F4 | P1 |
| FKEY-05 | `CapsLock + 5` | 模拟 F5 | P1 |
| FKEY-06 | `CapsLock + 6` | 模拟 F6 | P1 |
| FKEY-07 | `CapsLock + 7` | 模拟 F7 | P1 |
| FKEY-08 | `CapsLock + 8` | 模拟 F8 | P1 |
| FKEY-09 | `CapsLock + 9` | 模拟 F9 | P1 |
| FKEY-10 | `CapsLock + 0` | 模拟 F10 | P1 |
| FKEY-11 | `CapsLock + -` | 模拟 F11 | P1 |
| FKEY-12 | `CapsLock + =` | 模拟 F12 | P1 |

### 3.7 快捷功能（Quick Actions）

| ID | 快捷键 | 功能 | 优先级 |
|----|--------|------|--------|
| ACT-01 | `CapsLock + B` | 用默认浏览器搜索选中文本 | P2 |
| ACT-02 | `CapsLock + T` | 翻译选中文本（调用翻译 API / 打开翻译页面） | P2 |
| ACT-03 | `双按 CapsLock` | 打开快速启动器（应用搜索/启动） | P2 |
| ACT-04 | `CapsLock + O` | 快速打开文件/文件夹 | P2 |
| ACT-05 | `CapsLock + U` | 将选中文本转为大写 | P2 |
| ACT-06 | `CapsLock + Shift + U` | 将选中文本转为小写 | P2 |

### 3.8 系统托盘（System Tray）

| ID | 功能 | 优先级 |
|----|------|--------|
| TRAY-01 | 托盘图标常驻 | P0 |
| TRAY-02 | 左键单击：启用/禁用 CapslockX | P0 |
| TRAY-03 | 右键菜单：打开设置 / 关于 / 退出 | P0 |
| TRAY-04 | 状态指示（启用/暂停/出错） | P1 |

### 3.9 设置界面（Settings UI）

| ID | 功能 | 优先级 |
|----|------|--------|
| SET-01 | 快捷键查看与自定义绑定 | P1 |
| SET-02 | 长按阈值调节 | P1 |
| SET-03 | 开机自启开关 | P1 |
| SET-04 | 排除应用列表（黑名单） | P2 |
| SET-05 | 导入/导出配置文件 | P2 |
| SET-06 | 中/英文界面切换 | P2 |
| SET-07 | 自定义快捷键（添加新的绑定） | P2 |

---

## 4. 非功能需求

### 4.1 性能

| 指标 | 要求 |
|------|------|
| 按键响应延迟 | < 10ms（钩子回调执行时间） |
| 内存占用 | < 50MB（常驻后台） |
| CPU 空闲占用 | < 0.1% |
| 启动时间 | < 3s |

### 4.2 可靠性

| 指标 | 要求 |
|------|------|
| 崩溃率 | < 0.1% / session |
| 钩子失效自动恢复 | 检测到钩子被卸载后 3s 内重新注册 |
| 大小写指示灯 | 保持与系统状态同步 |

### 4.3 兼容性

| 场景 | 要求 |
|------|------|
| 游戏/全屏应用 | 自动检测并暂停功能（可配置） |
| 远程桌面 | 在 RDP 窗口内自动暂停或切换配置 |
| 虚拟机 | 在 VM 窗口内自动暂停 |
| 管理员权限应用 | 工具需以管理员权限运行才能注入钩子 |

### 4.4 安全性

| 要求 | 说明 |
|------|------|
| 不记录按键内容 | 仅在内存中处理按键事件，不落盘 |
| 不上传任何数据 | 纯本地运行 |
| 配置文件不存储敏感信息 | 快捷键绑定仅存储键码，不存储输入内容 |

---

## 5. 技术约束

### 5.1 实际技术选型

| 层面 | 方案 | 说明 |
|------|------|------|
| **核心语言** | C# (.NET 9) | D1 已决议 |
| **UI 框架** | WPF + WinForms (托盘) | WPF 宿主 + WinForms NotifyIcon |
| **键盘钩子** | Win32 `SetWindowsHookEx(WH_KEYBOARD_LL)` + P/Invoke | 全局低级钩子 |
| **模拟输入** | `SendInput` API（P/Invoke） | CapsLock 切换用 SendInput（LLKHF_INJECTED 防止钩子重入） |
| **配置存储** | 硬编码（Phase 1）；JSON（Phase 2+ 计划） | 当前绑定在 `BindingManager.cs` |
| **分发方式** | 单文件自包含发布 + 原生 DLL 内嵌 | 163MB，无需安装 .NET Runtime |

### 5.2 项目结构

### 5.2 项目结构

```
CapslockX/
├── CapslockX.sln
├── src/
│   ├── CapslockX.Core/              # 核心库（无 UI 依赖）
│   │   ├── Native/                  # Win32 P/Invoke
│   │   │   ├── NativeMethods.cs     # SetWindowsHookEx, SendInput, keybd_event...
│   │   │   ├── NativeStructs.cs     # INPUT, KBDLLHOOKSTRUCT, RECT...
│   │   │   └── NativeConstants.cs   # VK_*, WH_KEYBOARD_LL, flags...
│   │   ├── Hook/
│   │   │   └── KeyboardHook.cs      # WH_KEYBOARD_LL 封装
│   │   ├── StateMachine/
│   │   │   └── CapsLockStateMachine.cs  # 短按/长按/组合键三态区分
│   │   └── Bindings/
│   │       ├── KeyBinding.cs        # 键位绑定模型
│   │       ├── BindingManager.cs    # 绑定注册与查找（Phase 1 硬编码）
│   │       └── InputSimulator.cs    # SendInput 封装
│   └── CapslockX.App/               # WPF 宿主
│       ├── App.xaml / App.xaml.cs    # 入口，初始化钩子+状态机
│       ├── TrayIconService.cs       # 托盘图标（动态绘制）
│       └── MainWindow.xaml          # 占位（将来设置界面）
├── tests/
│   └── CapslockX.Core.Tests/        # 19 个单元测试（xUnit）
│       ├── CapsLockStateMachineTests.cs
│       └── BindingManagerTests.cs
└── publish/
    └── CapslockX.exe                # 单文件发布（163 MB）
```

### 5.3 CapsLock 状态机实现细节

核心策略：**完全拦截 CapsLock DOWN/UP，通过 SendInput 手动切换。**

1. WH_KEYBOARD_LL 钩子拦截 CapsLock 的 WM_KEYDOWN 和 WM_KEYUP
2. `Stopwatch` 计时区分短按（< 300ms）和长按（>= 300ms）
3. 其他键在 CapsLock 按住期间按下 → 立即进入修饰键模式
4. 修饰键（Shift/Ctrl/Alt/Win）放行，确保 `GetAsyncKeyState` 能检测到
5. CapsLock 切换通过 `SendInput` 模拟（LLKHF_INJECTED 标志防止钩子重入）

### 5.4 系统要求

| 项目 | 最低要求 |
|------|---------|
| 操作系统 | Windows 10 22H2 / Windows 11 |
| 架构 | x64 |
| 权限 | 管理员权限（安装时；运行时仅当目标窗口以管理员运行时需要） |
| .NET Runtime | 无需安装（自包含发布，运行时内嵌） |

---

## 6. 配置结构

### 6.1 默认配置文件（草案）

文件路径：`%APPDATA%/CapslockX/settings.json`

```jsonc
{
  "version": "1.0.0",
  "general": {
    "enabled": true,
    "startWithWindows": true,
    "longPressThresholdMs": 300,
    "language": "zh-CN",
    "checkUpdate": true
  },
  "excludedApps": [
    // 进程名列表，这些应用运行时自动暂停 CapslockX
    "vmware.exe",
    "VirtualBox.exe",
    "mstsc.exe",
    "steam.exe"
  ],
  "keybindings": {
    "cursor": {
      "up":          { "key": "E" },
      "down":        { "key": "D" },
      "left":        { "key": "S" },
      "right":       { "key": "F" },
      "home":        { "key": "A" },
      "end":         { "key": "G" },
      "wordLeft":    { "key": "W" },
      "wordRight":   { "key": "R" },
      "pageUp":      { "key": "Q" },
      "pageDown":    { "key": "C", "disabled": true }  // 与 editing.copy 冲突，默认禁用
    },
    "editing": {
      "cut":         { "key": "X" },
      "copy":        { "key": "C" },  // 注：与 cursor.pageDown 冲突，见待决策事项
      "paste":       { "key": "V" },
      "undo":        { "key": "Z" },
      "redo":        { "key": "Y" },
      "deleteLine":  { "key": "P" },
      "selectMode":  { "key": ";" }
    },
    "window": {
      "leftHalf":    { "key": "Left" },
      "rightHalf":   { "key": "Right" },
      "maximize":    { "key": "Up" },
      "restore":     { "key": "Down" },
      "toggleMax":   { "key": "Enter" },
      "minimize":    { "key": "M" },
      "center":      { "key": "/" }
    },
    "tab": {
      "prevTab":     { "key": "J" },
      "nextTab":     { "key": "L" },
      "newTab":      { "key": "T" },
      "closeTab":    { "key": "W", "disabled": true }   // 与 cursor.wordLeft 冲突，默认禁用
    },
    "functionKeys": {
      "F1":  { "key": "1" },
      "F2":  { "key": "2" },
      "F3":  { "key": "3" },
      "F4":  { "key": "4" },
      "F5":  { "key": "5" },
      "F6":  { "key": "6" },
      "F7":  { "key": "7" },
      "F8":  { "key": "8" },
      "F9":  { "key": "9" },
      "F10": { "key": "0" },
      "F11": { "key": "-" },
      "F12": { "key": "=" }
    },
    "quickActions": {
      "webSearch":   { "key": "B" },
      "translate":   { "key": "T" }
    }
  }
}
```

### 6.2 键值格式说明

| 字段 | 类型 | 说明 | 示例 |
|------|------|------|------|
| `key` | String | 虚拟键名，对应 Windows VK 码 | `"E"`, `"Space"`, `"Left"`, `";"` |
| `disabled` | Boolean | 设为 `true` 则禁用此项绑定 | `false` |

可用的特殊键名：`"Backspace"`, `"Delete"`, `"Space"`, `"Enter"`, `"Tab"`, `"Escape"`, `"Left"`, `"Right"`, `"Up"`, `"Down"`, 以及所有字母数字符号键。

---

## 7. 开发计划

### 7.0 Phase 1 已实现功能

| 功能 | 快捷键 | 说明 |
|------|--------|------|
| 短按 CapsLock | CapsLock（< 300ms） | 正常切换大小写 |
| 长按 CapsLock | CapsLock（>= 300ms） | 无反应，不切换 |
| 光标上移 | CapsLock + E | ↑ |
| 光标下移 | CapsLock + D | ↓ |
| 光标左移 | CapsLock + S | ← |
| 光标右移 | CapsLock + F | → |
| 行首 | CapsLock + A | Home |
| 行尾 | CapsLock + G | End |
| 选中文本 | CapsLock + Shift + E/D/S/F | Shift + 方向键 |
| 选中到行首/尾 | CapsLock + Shift + A/G | Shift + Home/End |
| 托盘图标 | 系统托盘 | 蓝色圆形 "CX" 图标 + 右键菜单 |
| 图标动态绘制 | 代码生成 | #2e59a3 圆形背景，Segoe UI Black 白色 "CX" |

### 7.1 里程碑

```
Phase 1 ─── MVP ─────────────────────── [已完成 ✅]
  ├── ✅ 全局键盘钩子（WH_KEYBOARD_LL）
  ├── ✅ 短按/长按/组合键三态区分
  ├── ✅ 光标移动（ESDF + AG）
  └── ✅ 基本文本选择（CapsLock + Shift + 方向键）

Phase 2 ─── 编辑增强 ────────────────── [预计 1-2 周]
  ├── 完整文本编辑快捷键（剪切/复制/粘贴/撤销）
  ├── 功能键模拟（F1-F12）
  └── Tab 切换

Phase 3 ─── 窗口管理 ────────────────── [预计 1-2 周]
  ├── 窗口贴边（左/右/上/下半屏）
  ├── 窗口最大/最小化
  ├── 窗口居中
  └── 多显示器支持

Phase 4 ─── 用户界面 ────────────────── [预计 2-3 周]
  ├── 系统托盘图标与菜单
  ├── 设置界面（快捷键查看/自定义）
  ├── 开机自启
  └── 排除应用列表

Phase 5 ─── 高级功能 ────────────────── [预计 2-3 周]
  ├── 快速启动器
  ├── 翻译
  ├── Web 搜索
  ├── 窗口置顶/透明度
  └── 九宫格窗口布局
```

### 7.2 测试计划

| 阶段 | 测试内容 |
|------|---------|
| 单元测试 | 键盘事件解析、配置加载/保存、按键时长判断逻辑 |
| 集成测试 | 钩子注册/卸载、SendInput 模拟准确性、窗口管理 API |
| 兼容性测试 | Win10/Win11、多显示器、高 DPI、各种输入法、远程桌面 |
| 性能测试 | 钩子回调延迟测量、长稳测试（24h+） |

---

## 8. 待决策事项

### 8.1 技术决策

| # | 事项 | 状态 |
|---|------|------|
| D1 | **开发语言** | ✅ C# + .NET 9 + WPF |
| D2 | **UI 框架** | ✅ WPF（宿主）+ WinForms（NotifyIcon 托盘） |
| D3 | **配置格式** | ⬜ Phase 2 |

### 8.2 功能决策

| # | 事项 | 状态 |
|---|------|------|
| D4 | **CapsLock 行为** | ✅ 短按切换；长按不切换；组合键不切换（全拦截+SendInput 手动切换） |
| D5 | **应用排除策略** | ⬜ Phase 4 |
| D6 | **按键冲突处理** | ✅ 冲突项默认禁用 |
| D7 | **用户自定义程度** | ⬜ Phase 2（JSON 配置） |
| D8 | **启动方式** | ⬜ Phase 4（开机自启） |

### 8.3 产品决策

| # | 事项 | 状态 |
|---|------|------|
| D9 | **开源策略** | ⬜ 待定 |
| D10 | **产品命名** | ✅ CapslockX |

---

## 9. 变更记录

| 日期 | 版本 | 变更内容 | 作者 |
|------|------|---------|------|
| 2026-06-03 | v0.1.0 | 初始草案，基于 capslox.com 和 capslock-plus 调研 | Claude |
| 2026-06-03 | v0.3.0 | D1 决议：C# + .NET 9 + WPF/WinUI 3 | Claude |
| 2026-06-04 | v1.0.0 | Phase 1 MVP 完成：光标移动、文本选择、托盘图标、19 单元测试、单文件发布 | Claude |

---

> **下一步**: Phase 2 — 编辑增强（剪切/复制/粘贴/撤销 + 功能键 F1-F12 + Tab 切换）