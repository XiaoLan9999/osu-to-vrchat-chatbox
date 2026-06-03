namespace OsuOscVRC.Config
{
    public class AppConfig
    {
        public string TosuExePath { get; set; } = "tosu/tosu.exe";
        public string TosuHost { get; set; } = "127.0.0.1";
        public int TosuPort { get; set; } = 24050;
        public string VRChatOscHost { get; set; } = "127.0.0.1";
        public int VRChatOscPort { get; set; } = 9000;

        public int UpdateIntervalMs { get; set; } = 2689;
        public int ResultScreenDurationS { get; set; } = 0;
        public int PauseDetectionThresholdMs { get; set; } = 500;
        public int ReconnectDelayMs { get; set; } = 5000;
        public int MaxMessageLength { get; set; } = 144;
        public int MaxTitleLength { get; set; } = 50;

        public bool UseUnicodeTitle { get; set; } = false;
        public bool ShowArtist { get; set; } = false;
        public string HiddenMods { get; set; } = "";
        public int StarDecimals { get; set; } = 2;
        public int PpDecimals { get; set; } = 1;
        public int AccuracyDecimals { get; set; } = 2;

        public TemplatesConfig Templates { get; set; } = new();
        public ModeNamesConfig ModeNames { get; set; } = new();
        public string Language { get; set; } = "auto";
        public bool FirstRunDone { get; set; } = false;
    }

    public class TemplatesConfig
    {
        public string PlayingLine1 { get; set; } = "Playing osu!{mode} {title} [{version}] {stars}⭐";
        public string PlayingLine2 { get; set; } = "{time_current}/{time_total} {accuracy}% {combo}x {miss}miss {mods} {pp}PP";
        public string PausedPrefix { get; set; } = "[Paused] ";
        public string WatchingReplay { get; set; } = "Watching osu!{mode} {title} [{version}] {stars}⭐ played by {player}";
        public string ReplayResult { get; set; } = "Replay result osu!{mode} {title} [{version}] {stars}⭐ {rank} {mods} {accuracy}% {miss}miss {pp}PP";
        public string SongSelect { get; set; } = "Selecting osu!{mode} {title} [{version}] {stars}⭐";
        public string Editor { get; set; } = "Editing osu!{mode} {title} [{version}]";
        public string ResultScreen { get; set; } = "[Cleared!] osu!{mode} {title} [{version}] {stars}⭐ {mods} {rank} {accuracy}% {miss}miss Get {pp}PP";
        public string IdleText { get; set; } = "In osu! Lobby";
        public string NotRunning { get; set; } = "";
    }

    public class ModeNamesConfig
    {
        public string Osu { get; set; } = "";
        public string Taiko { get; set; } = "taiko";
        public string Catch { get; set; } = "catch";
        public string Mania { get; set; } = "mania";
    }
}
