using System;
using System.Text.RegularExpressions;
using OsuOscVRC.Config;
using OsuOscVRC.Data;

namespace OsuOscVRC.Formatter
{
    public static class ChatboxFormatter
    {
        private static readonly HashSet<string> KnownModAcronyms = new(StringComparer.OrdinalIgnoreCase)
        {
            "NF", "EZ", "TD", "HD", "HR", "SD", "DT", "RX", "HT", "NC", "FL", "AT", "SO", "AP", "PF",
            "4K", "5K", "6K", "7K", "8K", "FI", "RD", "CN", "TP", "9K", "CO", "1K", "3K", "2K", "V2",
            "MR", "10K", "CL", "DA", "BL", "ST", "AC", "AL", "SG", "MG", "RP", "AS", "TR", "WG",
            "SI", "GR", "DF", "WU", "BR", "AD", "MU", "NS", "DP", "BM"
        };

        private static readonly Dictionary<string, string> ModAliases = new(StringComparer.OrdinalIgnoreCase)
        {
            ["NOFAIL"] = "NF",
            ["EASY"] = "EZ",
            ["TOUCHDEVICE"] = "TD",
            ["HIDDEN"] = "HD",
            ["HARDROCK"] = "HR",
            ["SUDDENDEATH"] = "SD",
            ["DOUBLETIME"] = "DT",
            ["RELAX"] = "RX",
            ["HALFTIME"] = "HT",
            ["NIGHTCORE"] = "NC",
            ["FLASHLIGHT"] = "FL",
            ["AUTOPLAY"] = "AT",
            ["AUTO"] = "AT",
            ["SPUNOUT"] = "SO",
            ["AUTOPILOT"] = "AP",
            ["PERFECT"] = "PF",
            ["FADEIN"] = "FI",
            ["RANDOM"] = "RD",
            ["CINEMA"] = "CN",
            ["TARGET"] = "TP",
            ["COOP"] = "CO",
            ["SCOREV2"] = "V2",
            ["MIRROR"] = "MR",
            ["KEY1"] = "1K",
            ["KEY2"] = "2K",
            ["KEY3"] = "3K",
            ["KEY4"] = "4K",
            ["KEY5"] = "5K",
            ["KEY6"] = "6K",
            ["KEY7"] = "7K",
            ["KEY8"] = "8K",
            ["KEY9"] = "9K",
            ["KEY10"] = "10K"
        };

        public static string Format(OsuState? state, GameState gameState, AppConfig config, bool forceTimeZero = false)
        {
            if (state == null) return "";

            string template;
            switch (gameState)
            {
                case GameState.NotRunning:
                    return config.Templates.NotRunning ?? "";
                case GameState.Idle:
                case GameState.Menu:
                    template = config.Templates.IdleText ?? "";
                    if (string.IsNullOrEmpty(template)) return "";
                    break;
                case GameState.SongSelect:
                    template = config.Templates.SongSelect ?? "";
                    break;
                case GameState.Editor:
                    template = config.Templates.Editor ?? "";
                    break;
                case GameState.WatchingReplay:
                    template = $"{config.Templates.WatchingReplay}\n{config.Templates.PlayingLine2}";
                    break;
                case GameState.Playing:
                    template = $"{config.Templates.PlayingLine1}\n{config.Templates.PlayingLine2}";
                    break;
                case GameState.Failed:
                    template = $"[Failed] {config.Templates.PlayingLine1}\n{config.Templates.PlayingLine2}";
                    break;
                case GameState.Paused:
                    template = $"{config.Templates.PausedPrefix}{config.Templates.PlayingLine1}\n{config.Templates.PlayingLine2}";
                    break;
                case GameState.ReplayResultScreen:
                    template = config.Templates.ReplayResult ?? "";
                    break;
                case GameState.FailedResultScreen:
                    template = $"[Failed] {config.Templates.ResultScreen}";
                    break;
                case GameState.ResultScreen:
                    template = config.Templates.ResultScreen ?? "";
                    break;
                default:
                    return "";
            }

            return ApplyVariables(template, state, gameState, config, forceTimeZero);
        }

        private static string ApplyVariables(string template, OsuState state, GameState gameState, AppConfig config, bool forceTimeZero = false)
        {
            var title = config.UseUnicodeTitle && !string.IsNullOrEmpty(state.Beatmap?.TitleUnicode)
                ? state.Beatmap.TitleUnicode
                : (state.Beatmap?.Title ?? "");

            var artist = config.UseUnicodeTitle && !string.IsNullOrEmpty(state.Beatmap?.ArtistUnicode)
                ? state.Beatmap.ArtistUnicode
                : (state.Beatmap?.Artist ?? "");

            if (config.ShowArtist && !string.IsNullOrEmpty(artist))
                title = $"{artist} - {title}";

            // Truncate title and version to MaxTitleLength
            int maxLen = config.MaxTitleLength > 0 ? config.MaxTitleLength : 50;
            if (title.Length > maxLen)
                title = title.Substring(0, maxLen) + "…";

            string version = state.Beatmap?.Version ?? "";
            if (version.Length > maxLen)
                version = version.Substring(0, maxLen) + "…";

            // Mode - tosu uses "Osu", "Taiko", "Fruits" (=catch), "Mania"
            bool isResult = gameState == GameState.ResultScreen
                || gameState == GameState.ReplayResultScreen
                || gameState == GameState.FailedResultScreen;
            var mode = isResult
                ? (state.ResultsScreen?.Mode?.Name ?? "").ToLower()
                : (state.Play?.Mode?.Name ?? "").ToLower();
            // Fallback to settings.mode if play.mode is empty
            if (string.IsNullOrEmpty(mode))
                mode = (state.Settings?.Mode?.Name ?? "").ToLower();

            var modeName = mode switch
            {
                "taiko" => config.ModeNames.Taiko,
                "catch" or "fruits" => config.ModeNames.Catch,
                "mania" => config.ModeNames.Mania,
                _ => config.ModeNames.Osu
            };

            double accuracy = isResult ? (state.ResultsScreen?.Accuracy ?? 0) : (state.Play?.Accuracy ?? 0);
            double pp = isResult ? (state.ResultsScreen?.Pp?.Current ?? 0) : (state.Play?.Pp?.Current ?? 0);
            string rank = isResult ? (state.ResultsScreen?.Rank ?? "") : (state.Play?.Rank?.Current ?? "");
            int miss = isResult ? (state.ResultsScreen?.Hits?.CountMiss ?? 0) : (state.Play?.Hits?.CountMiss ?? 0);

            // Mode numeric ID and mods numeric ID
            int modeId = isResult
                ? (state.ResultsScreen?.Mode?.Number ?? state.Settings?.Mode?.Number ?? 0)
                : (state.Play?.Mode?.Number ?? state.Settings?.Mode?.Number ?? 0);
            int modsId = isResult
                ? (state.ResultsScreen?.Mods?.Number ?? 0)
                : (state.Play?.Mods?.Number ?? 0);

            // Hit counts (with isResult logic)
            int n300 = isResult ? (state.ResultsScreen?.Hits?.Count300 ?? 0) : (state.Play?.Hits?.Count300 ?? 0);
            int n100 = isResult ? (state.ResultsScreen?.Hits?.Count100 ?? 0) : (state.Play?.Hits?.Count100 ?? 0);
            int n50 = isResult ? (state.ResultsScreen?.Hits?.Count50 ?? 0) : (state.Play?.Hits?.Count50 ?? 0);
            int ngeki = isResult ? (state.ResultsScreen?.Hits?.Geki ?? 0) : (state.Play?.Hits?.Geki ?? 0);
            int nkatu = isResult ? (state.ResultsScreen?.Hits?.Katu ?? 0) : (state.Play?.Hits?.Katu ?? 0);
            int passedObjects = n300 + n100 + n50 + miss + ngeki + nkatu;

            // Clock rate: DT(64)/NC(512)=1.5, HT(256)=0.75, else 1.0
            double clockRate = 1.0;
            if ((modsId & 64) != 0 || (modsId & 512) != 0) clockRate = 1.5;
            else if ((modsId & 256) != 0) clockRate = 0.75;

            var timeCurrent = forceTimeZero ? "0:00" : FormatTime(state.Beatmap?.Time?.Live ?? 0);
            var timeTotal = FormatTime(state.Beatmap?.Time?.LastObject ?? 0);
            string mods = state.Play?.Mods?.Name ?? "";
            if (isResult && !string.IsNullOrWhiteSpace(state.ResultsScreen?.Mods?.Name))
                mods = state.ResultsScreen.Mods.Name;
            mods = FilterMods(mods, config.HiddenMods);

            double stars = state.Beatmap?.Stats?.Stars?.Total ?? 0;
            if (stars == 0) stars = state.Beatmap?.Stats?.Stars?.Live ?? 0;
            string starsStr = stars.ToString($"F{config.StarDecimals}");
            string accStr = accuracy.ToString($"F{config.AccuracyDecimals}");
            string ppStr = Math.Round(pp, config.PpDecimals).ToString($"F{config.PpDecimals}");
            string ppFcStr = Math.Round(state.Play?.Pp?.Fc ?? 0, config.PpDecimals).ToString($"F{config.PpDecimals}");
            string path = state.DirectPath?.BeatmapFile ?? "";
            string clockRateStr = clockRate.ToString("F2");

            var result = template
                .Replace("{title}", title)
                .Replace("{artist}", artist)
                .Replace("{version}", version)
                .Replace("{stars}", starsStr)
                .Replace("{mode}", modeName)
                .Replace("{time_current}", timeCurrent)
                .Replace("{time_total}", timeTotal)
                .Replace("{accuracy}", accStr)
                .Replace("{pp}", ppStr)
                .Replace("{pp_fc}", ppFcStr)
                .Replace("{rank}", rank)
                .Replace("{mods}", mods)
                .Replace("{combo}", (state.Play?.Combo?.Current ?? 0).ToString())
                .Replace("{max_combo}", (state.Play?.Combo?.Max ?? 0).ToString())
                .Replace("{miss}", miss.ToString())
                .Replace("{path}", path)
                .Replace("{mode_id}", modeId.ToString())
                .Replace("{mods_id}", modsId.ToString())
                .Replace("{acc}", accStr)
                .Replace("{n300}", n300.ToString())
                .Replace("{n100}", n100.ToString())
                .Replace("{n50}", n50.ToString())
                .Replace("{ngeki}", ngeki.ToString())
                .Replace("{nkatu}", nkatu.ToString())
                .Replace("{passed_objects}", passedObjects.ToString())
                .Replace("{clock_rate}", clockRateStr)
                .Replace("{player}", state.Play?.PlayerName ?? "");

            bool isWhiteSpace = !string.IsNullOrEmpty(result) && string.IsNullOrWhiteSpace(result);
            result = Regex.Replace(result, @"  +", " ");
            result = string.Join("\n", Array.ConvertAll(result.Split('\n'), l => l.Trim()));
            if (isWhiteSpace && result == "") result = " ";

            if (result.Length > config.MaxMessageLength)
            {
                if (config.MaxMessageLength <= 3)
                    result = result.Substring(0, config.MaxMessageLength);
                else
                    result = result.Substring(0, config.MaxMessageLength - 3) + "...";
            }

            return result;
        }

        private static string FilterMods(string mods, string? hiddenMods)
        {
            if (string.IsNullOrWhiteSpace(mods) || string.IsNullOrWhiteSpace(hiddenMods))
                return mods;

            var hidden = new HashSet<string>(
                SplitMods(hiddenMods),
                StringComparer.OrdinalIgnoreCase);

            if (hidden.Count == 0)
                return mods;

            var modNames = SplitMods(mods)
                .ToArray();

            if (!modNames.Any(hidden.Contains))
                return mods;

            return string.Join(",", modNames.Where(mod => !hidden.Contains(mod)));
        }

        private static IEnumerable<string> SplitMods(string mods)
        {
            return Regex.Split(mods, @"[\s,;+|/]+")
                .Where(mod => !string.IsNullOrWhiteSpace(mod))
                .SelectMany(SplitModToken);
        }

        private static IEnumerable<string> SplitModToken(string token)
        {
            var normalized = NormalizeModName(token);

            if (KnownModAcronyms.Contains(normalized))
            {
                yield return normalized;
                yield break;
            }

            var index = 0;
            while (index < normalized.Length)
            {
                string? match = null;
                foreach (var length in new[] { 3, 2 })
                {
                    if (index + length > normalized.Length)
                        continue;

                    var candidate = normalized.Substring(index, length);
                    if (KnownModAcronyms.Contains(candidate))
                    {
                        match = candidate;
                        break;
                    }
                }

                if (match == null)
                {
                    yield return normalized;
                    yield break;
                }

                yield return match;
                index += match.Length;
            }
        }

        private static string NormalizeModName(string mod)
        {
            var normalized = mod.Trim().ToUpperInvariant();
            return ModAliases.TryGetValue(normalized, out var alias) ? alias : normalized;
        }

        private static string FormatTime(int ms)
        {
            if (ms < 0) return "0:00";
            var ts = TimeSpan.FromMilliseconds(ms);
            return $"{(int)ts.TotalMinutes}:{ts.Seconds:D2}";
        }
    }
}
