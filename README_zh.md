# osu! → VRChat Chatbox

将 osu! 的实时游戏数据通过 OSC 发送到 VRChat 聊天框。

自动启动 tosu、连接 WebSocket、按模板格式化文字并发送到 VRChat。

## 功能

- 自动启动并连接 [tosu](https://github.com/tosuapp/tosu)
- 支持多种游戏状态：选歌、游玩、暂停、失败、结算、回放、编辑、空闲
- 每个状态均可自定义聊天框模板，支持模板变量
- 支持 osu! / taiko / catch / mania 四种模式，模式名可自定义
- 可按名称单独隐藏不想显示的 MOD
- 中文 / English 界面（跟随系统语言）

## 使用

1. 下载最新 Release 并解压
2. 确保 `tosu/tosu.exe` 存在（或在程序中手动指定路径）
3. 打开 VRChat 的 OSC 功能（操作 → OSC → 启用）
4. 启动 `OsuOscVRC.exe`，点击**开始**

启动后应用会自动启动 tosu、连接其 WebSocket，并向 `127.0.0.1:9000` 发送聊天框消息。

## 从源码运行

需要 .NET 8 SDK。

```powershell
dotnet build OsuOscVRC.csproj
dotnet run --project OsuOscVRC.csproj
```

## 配置

首次运行时生成 `config_osuosc.yaml`，所有设置均可在 UI 中修改。

在「显示」选项卡的「隐藏 MOD」中填写要从 `{mods}` 隐藏的名称，多个名称使用逗号或空格分隔。例如填写 `RELAX, HD` 会隐藏 RELAX/RX 和 HD，但不会改变 `{mods_id}` 或其他游戏数据。

### 模板变量

```
{title}         {artist}        {version}       {stars}          {mode}
{mode_id}       {path}          {time_current}  {time_total}
{accuracy}      {acc}           {pp}            {pp_fc}
{rank}          {mods}          {mods_id}       {combo}
{max_combo}     {miss}          {player}
{n300}          {n100}          {n50}           {ngeki}
{nkatu}         {passed_objects}                {clock_rate}
```

- `{mode}` — 模式名称（可在「显示」选项卡中自定义）。默认：osu! = `""`、taiko = `"taiko"`、catch = `"catch"`、mania = `"mania"`。
- `{mode_id}` — 模式数字 ID：0=osu!、1=Taiko、2=Catch、3=Mania。
- `{path}` — .osu 谱面文件的完整路径。
- `{accuracy}` / `{acc}` — 准确率 (0–100)。
- `{mods}` — 经过「隐藏 MOD」过滤后的模组名称字符串（如 `"DT,HD"`）。
- `{mods_id}` — 模组位运算数字 ID（如 `64` = DT）。
- `{miss}` — 失误次数。
- `{player}` — 观看回放时显示回放对象的名字，其他状态显示自己的名字。
- `{n300}`、`{n100}`、`{n50}`、`{ngeki}`、`{nkatu}` — 各判定数量。
- `{passed_objects}` — 总通过物量 (n300 + n100 + n50 + miss + ngeki + nkatu)。
- `{clock_rate}` — 由模组决定的变速倍率 (1.0 / 1.5 / 0.75)。

### 默认模板

| 状态 | 第一行 | 第二行 |
|------|--------|--------|
| 游玩 | `Playing osu!{mode} {title} [{version}] {stars}⭐` | `{time_current}/{time_total} {accuracy}% {combo}x {miss}miss {mods} {pp}PP` |
| 暂停 | `[Paused] ` + 游玩第一行 | `{time_current}/{time_total} {accuracy}% {combo}x {miss}miss {mods} {pp}PP` |
| 失败 | `[Failed] ` + 游玩第一行 | `{time_current}/{time_total} {accuracy}% {combo}x {miss}miss {mods} {pp}PP` |
| 选歌 | `Selecting osu!{mode} {title} [{version}] {stars}⭐` | *(单行)* |
| 结算 | `[Cleared!] osu!{mode} {title} [{version}] {stars}⭐ {mods} {rank} {accuracy}% {miss}miss Get {pp}PP` | *(单行)* |
| 回放结算 | `Replay result osu!{mode} {title} [{version}] {stars}⭐ {rank} {mods} {accuracy}% {miss}miss {pp}PP` | *(单行)* |
| 观看回放 | `Watching osu!{mode} {title} [{version}] {stars}⭐ played by {player}` | `{time_current}/{time_total} {accuracy}% {combo}x {miss}miss {mods} {pp}PP` |
| 编辑 | `Editing osu!{mode} {title} [{version}]` | *(单行)* |
| 空闲 | `In osu! Lobby` | *(单行)* |

> 游玩相关状态（游玩、暂停、失败、观看回放）共用「游玩第二行」模板设置。

## 构建 Release 包

推送 `v*` 标签时 GitHub Actions 自动构建并打包。

手动构建：

```powershell
dotnet publish OsuOscVRC.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish
```

## 注意事项

请尽量在私人房间使用。频繁变动的聊天框文本在公共场所可能打扰他人。

## 免责声明

本项目与 VRChat、ppy、tosu 无关，为非官方第三方工具。

---

*使用 DeepSeek V4 Pro + Claude Code 完成 — 花费 6 RMB。*
