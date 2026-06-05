using System.Globalization;

namespace OsuOscVRC.I18n
{
    public static class Translator
    {
        private static string _language = "auto";

        public static string Language
        {
            get => _language;
            set => _language = value;
        }

        public static string EffectiveLanguage
        {
            get
            {
                if (_language != "auto") return _language;
                var culture = CultureInfo.CurrentUICulture.Name.ToLower();
                if (culture.StartsWith("zh")) return "zh";
                if (culture.StartsWith("ja")) return "ja";
                return "en";
            }
        }

        public static string Get(string key) => EffectiveLanguage switch
        {
            "zh" => ZH(key),
            "ja" => JP(key),
            _ => EN(key)
        };

        private static string ZH(string k) => k switch
        {
            "AdvLanguage" => "语言:",
            "Title" => "osu! → VRChat OSC",
            "StatusConnected" => "● 已连接 | tosu 运行中 | OSC 活动中",
            "StatusDisconnected" => "○ 未连接 | 等待 tosu...",
            "StatusTosuDisconnected" => "○ 正在连接 tosu...",
            "StatusStartingTosu" => "○ 正在启动 tosu...",
            "StatusOscStopped" => "● tosu 运行中 | OSC 已暂停",
            "PreviewLabel" => "当前输出预览:",
            "Start" => "▶ 开始",
            "StopOsc" => "⏸ 停止OSC",
            "Stop" => "⏹ 停止",
            "SaveConfig" => "保存配置",
            "ConfigSaved" => "配置已保存！",
            "FirstRunTitle" => "⚠ 注意事项 / NOTICE",
            "FirstRunMessage" => "请尽量在私人房间使用本软件，以免影响他人游玩体验。\nPlease use this software in PRIVATE rooms to avoid disturbing others' gameplay experience.",
            "TabConnection" => "连接",
            "TabTemplates" => "模板",
            "TabDisplay" => "显示",
            "TabAdvanced" => "高级",
            "TosuExePath" => "tosu.exe 路径:",
            "TosuPort" => "tosu 端口:",
            "OscHost" => "VRC OSC 地址:",
            "OscPort" => "VRC OSC 端口:",
            "TplPlaying1" => "游玩中 (第一行):",
            "TplPlaying2" => "游玩中 (第二行):",
            "TplPaused" => "暂停前缀:",
            "TplReplay" => "观看回放:",
            "TplReplayResult" => "回放结算:",
            "TplSongSelect" => "选歌界面:",
            "TplEditor" => "编辑模式:",
            "TplResult" => "结算界面:",
            "TplIdle" => "大厅/空闲:",
            "DispUseUnicode" => "使用原版标题 (Unicode)",
            "DispShowArtist" => "显示艺术家名称",
            "DispHiddenMods" => "隐藏 MOD:",
            "DispHiddenModsHint" => "用逗号或空格分隔，例如 RELAX, HD。也支持 HDRX 这类连写 MOD。",
            "DispStarDecimals" => "星数小数位:",
            "DispPpDecimals" => "PP 小数位:",
            "DispAccDecimals" => "准确度小数位:",
            "DispModeOsu" => "osu! 模式:",
            "DispModeTaiko" => "taiko 模式:",
            "DispModeCatch" => "catch 模式:",
            "DispModeMania" => "mania 模式:",
            "AdvOscRate" => "OSC 发送间隔 (ms):",
            "AdvResultDuration" => "结算停留 (s):",
            "AdvPauseThreshold" => "暂停阈值 (ms):",
            "AdvReconnectDelay" => "重连延迟 (ms):",
            "AdvMaxLength" => "最大消息长度:",
            "AdvMaxTitleLen" => "标题最大长度:",
            "Browse" => "浏览...",
            "TosuNotFound" => "找不到 tosu.exe",
            "TosuWaitTimeout" => "tosu 启动超时，请确认路径正确后重试。",
            _ => k
        };

        private static string EN(string k) => k switch
        {
            "AdvLanguage" => "Language:",
            "Title" => "osu! → VRChat OSC",
            "StatusConnected" => "● Connected | tosu Running | OSC Active",
            "StatusDisconnected" => "○ Disconnected | Waiting for tosu...",
            "StatusTosuDisconnected" => "○ Connecting to tosu...",
            "StatusStartingTosu" => "○ Starting tosu...",
            "StatusOscStopped" => "● tosu Running | OSC Paused",
            "PreviewLabel" => "Current Output Preview:",
            "Start" => "▶ Start",
            "StopOsc" => "⏸ Stop OSC",
            "Stop" => "⏹ Stop",
            "SaveConfig" => "Save Config",
            "ConfigSaved" => "Configuration saved!",
            "FirstRunTitle" => "NOTICE / ⚠ 注意事项",
            "FirstRunMessage" => "Please use this software in PRIVATE rooms to avoid disturbing others' gameplay experience.\n请尽量在私人房间使用本软件，以免影响他人游玩体验。",
            "TabConnection" => "Connection",
            "TabTemplates" => "Templates",
            "TabDisplay" => "Display",
            "TabAdvanced" => "Advanced",
            "TosuExePath" => "tosu.exe Path:",
            "TosuPort" => "tosu Port:",
            "OscHost" => "VRC OSC Host:",
            "OscPort" => "VRC OSC Port:",
            "TplPlaying1" => "Playing (Line 1):",
            "TplPlaying2" => "Playing (Line 2):",
            "TplPaused" => "Paused Prefix:",
            "TplReplay" => "Watching Replay:",
            "TplReplayResult" => "Replay Result:",
            "TplSongSelect" => "Song Select:",
            "TplEditor" => "Editor Mode:",
            "TplResult" => "Result Screen:",
            "TplIdle" => "Idle/Lobby:",
            "DispUseUnicode" => "Use Unicode Titles",
            "DispShowArtist" => "Show Artist",
            "DispHiddenMods" => "Hide Mods:",
            "DispHiddenModsHint" => "Separate names with commas or spaces, for example RELAX, HD. Joined strings such as HDRX are supported too.",
            "DispStarDecimals" => "Star Decimals:",
            "DispPpDecimals" => "PP Decimals:",
            "DispAccDecimals" => "Accuracy Decimals:",
            "DispModeOsu" => "osu! Mode:",
            "DispModeTaiko" => "taiko Mode:",
            "DispModeCatch" => "catch Mode:",
            "DispModeMania" => "mania Mode:",
            "AdvOscRate" => "OSC Send Rate (ms):",
            "AdvResultDuration" => "Result Duration (s):",
            "AdvPauseThreshold" => "Pause Threshold (ms):",
            "AdvReconnectDelay" => "Reconnect Delay (ms):",
            "AdvMaxLength" => "Max Message Length:",
            "AdvMaxTitleLen" => "Max Title Length:",
            "Browse" => "Browse...",
            "TosuNotFound" => "tosu.exe not found",
            "TosuWaitTimeout" => "tosu startup timed out. Verify the path and try again.",
            _ => k
        };

        private static string JP(string k) => k switch
        {
            "AdvLanguage" => "言語:",
            "Title" => "osu! → VRChat OSC",
            "StatusConnected" => "● 接続完了 | tosu 実行中 | OSC アクティブ",
            "StatusDisconnected" => "○ 未接続 | tosu を待機中...",
            "StatusTosuDisconnected" => "○ tosu に接続中...",
            "StatusStartingTosu" => "○ tosu を起動中...",
            "StatusOscStopped" => "● tosu 実行中 | OSC 停止中",
            "PreviewLabel" => "現在の出力プレビュー:",
            "Start" => "▶ 開始",
            "StopOsc" => "⏸ OSC停止",
            "Stop" => "⏹ 停止",
            "SaveConfig" => "設定を保存",
            "ConfigSaved" => "設定を保存しました！",
            "FirstRunTitle" => "⚠ 注意事項 / NOTICE",
            "FirstRunMessage" => "本ソフトウェアはプライベートルームでご使用ください。他のプレイヤーのプレイ体験に影響を与えないようお願いします。\nPlease use this software in PRIVATE rooms to avoid disturbing others' gameplay experience.",
            "TabConnection" => "接続",
            "TabTemplates" => "テンプレート",
            "TabDisplay" => "表示",
            "TabAdvanced" => "詳細設定",
            "TosuExePath" => "tosu.exe パス:",
            "TosuPort" => "tosu ポート:",
            "OscHost" => "VRC OSC ホスト:",
            "OscPort" => "VRC OSC ポート:",
            "TplPlaying1" => "プレイ中 (1行目):",
            "TplPlaying2" => "プレイ中 (2行目):",
            "TplPaused" => "一時停止プレフィックス:",
            "TplReplay" => "リプレイ視聴:",
            "TplReplayResult" => "リプレイ結果:",
            "TplSongSelect" => "曲選択:",
            "TplEditor" => "エディターモード:",
            "TplResult" => "結果画面:",
            "TplIdle" => "ロビー/待機中:",
            "DispUseUnicode" => "オリジナルタイトルを使用 (Unicode)",
            "DispShowArtist" => "アーティスト名を表示",
            "DispHiddenMods" => "非表示 MOD:",
            "DispHiddenModsHint" => "カンマまたは空白で区切ります。例: RELAX, HD。HDRX のような連結 MOD にも対応します。",
            "DispStarDecimals" => "レート小数桁:",
            "DispPpDecimals" => "PP 小数桁:",
            "DispAccDecimals" => "精度 小数桁:",
            "DispModeOsu" => "osu! モード名:",
            "DispModeTaiko" => "taiko モード名:",
            "DispModeCatch" => "catch モード名:",
            "DispModeMania" => "mania モード名:",
            "AdvOscRate" => "OSC 送信間隔 (ms):",
            "AdvResultDuration" => "結果表示時間 (s):",
            "AdvPauseThreshold" => "一時停止判定 (ms):",
            "AdvReconnectDelay" => "再接続遅延 (ms):",
            "AdvMaxLength" => "最大メッセージ長:",
            "AdvMaxTitleLen" => "タイトル最大長:",
            "Browse" => "参照...",
            "TosuNotFound" => "tosu.exe が見つかりません",
            "TosuWaitTimeout" => "tosu の起動がタイムアウトしました。パスを確認して再試行してください。",
            _ => k
        };
    }
}
