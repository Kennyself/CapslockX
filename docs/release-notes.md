# CapslockX Release Notes

---

## v1.0.0 — MVP (2026-06-04)

### 新增功能

- **CapsLock 修饰键**：按住 CapsLock 进入修饰键模式，配合其他键触发快捷键
- **光标移动**：CapsLock + E/D/S/F（↑↓←→）、A（Home）、G（End）
- **文本选择**：CapsLock + Shift + E/D/S/F/A/G（选中文本）
- **大小写切换**：短按 CapsLock（< 0.3s）正常切换，长按（> 0.3s）不切换
- **系统托盘**：蓝色圆形 "CX" 图标，右键菜单（About / Exit）
- **单文件发布**：163MB 自包含 exe，无需安装 .NET Runtime

### 技术架构

- C# (.NET 9) + WPF + WinForms
- WH_KEYBOARD_LL 全局低级键盘钩子
- CapsLock 事件全拦截 + SendInput 手动切换（LLKHF_INJECTED 防重入）
- 19 个单元测试（xUnit）

### 已知限制

- 需以管理员身份运行
- 未支持全屏应用/游戏自动暂停
- 快捷键硬编码，暂不支持自定义

### 文件

| 文件 | 说明 |
|------|------|
| `CapslockX.exe` | 主程序（163 MB） |
| `docs/user-guide.md` | 用户手册 |
| `docs/requirements.md` | 需求规格说明书 |

---

## v1.1.0 — 计划中

### 计划加入

- 剪切/复制/粘贴/撤销（CapsLock + X/C/V/Z/Y）
- 功能键模拟（CapsLock + 1~0/-/= → F1~F12）
- Tab 切换（CapsLock + J/L）
- 按词跳转（CapsLock + W/R → Ctrl+←/→）
- JSON 配置文件支持