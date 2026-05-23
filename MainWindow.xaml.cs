using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Win32;
using OsuOscVRC.Config;
using OsuOscVRC.Data;
using OsuOscVRC.Formatter;
using OsuOscVRC.I18n;
using OsuOscVRC.OSC;

namespace OsuOscVRC
{
    public partial class MainWindow : Window
    {
        private bool _isInitializing = true;
        private AppConfig _config;
        private TosuProcessManager? _tosuProcess;
        private TosuWebSocketClient? _tosuClient;
        private VRChatOscSender? _oscSender;
        private DispatcherTimer _updateTimer;

        private GameState _currentGameState = GameState.NotRunning;
        private bool _wasWatchingReplay;
        private bool _lastPlayFailed;
        private int _lastTimeLive = -1;
        private DateTime _lastTimeLiveChanged = DateTime.MinValue;
        private DateTime _resultScreenStartTime = DateTime.MinValue;

        public MainWindow()
        {
            InitializeComponent();
            _config = ConfigManager.Load();
            Translator.Language = _config.Language;
            ApplyI18n();
            ConfigManager.CheckFirstRun(_config);
            PopulateUiFromConfig();
            _isInitializing = false;
            _updateTimer = new DispatcherTimer();
            _updateTimer.Tick += UpdateTimer_Tick;
            UpdateStatus();
        }

        private void ApplyI18n()
        {
            Title = Translator.Get("Title");
            GrpPreview.Header = Translator.Get("PreviewLabel");
            BtnStart.Content = Translator.Get("Start");
            BtnStopOsc.Content = Translator.Get("StopOsc");
            BtnStop.Content = Translator.Get("Stop");
            BtnSave.Content = Translator.Get("SaveConfig");
            TabConnection.Header = Translator.Get("TabConnection");
            TabTemplates.Header = Translator.Get("TabTemplates");
            TabDisplay.Header = Translator.Get("TabDisplay");
            TabAdvanced.Header = Translator.Get("TabAdvanced");
            LblTosuExePath.Content = Translator.Get("TosuExePath");
            LblTosuPort.Content = Translator.Get("TosuPort");
            LblOscHost.Content = Translator.Get("OscHost");
            LblOscPort.Content = Translator.Get("OscPort");
            LblTplPlaying1.Content = Translator.Get("TplPlaying1");
            LblTplPlaying2.Content = Translator.Get("TplPlaying2");
            LblTplPaused.Content = Translator.Get("TplPaused");
            LblTplReplay.Content = Translator.Get("TplReplay");
            LblTplReplayResult.Content = Translator.Get("TplReplayResult");
            LblTplSongSelect.Content = Translator.Get("TplSongSelect");
            LblTplResult.Content = Translator.Get("TplResult");
            LblTplEditor.Content = Translator.Get("TplEditor");
            LblTplIdle.Content = Translator.Get("TplIdle");
            CbUseUnicode.Content = Translator.Get("DispUseUnicode");
            CbShowArtist.Content = Translator.Get("DispShowArtist");
            LblDispStar.Content = Translator.Get("DispStarDecimals");
            LblDispPp.Content = Translator.Get("DispPpDecimals");
            LblDispAcc.Content = Translator.Get("DispAccDecimals");
            LblDispModeOsu.Content = Translator.Get("DispModeOsu");
            LblDispModeTaiko.Content = Translator.Get("DispModeTaiko");
            LblDispModeCatch.Content = Translator.Get("DispModeCatch");
            LblDispModeMania.Content = Translator.Get("DispModeMania");
            LblAdvUpdate.Content = Translator.Get("AdvOscRate");
            LblAdvResult.Content = Translator.Get("AdvResultDuration");
            LblAdvPause.Content = Translator.Get("AdvPauseThreshold");
            LblAdvReconnect.Content = Translator.Get("AdvReconnectDelay");
            LblAdvMaxLen.Content = Translator.Get("AdvMaxLength");
            LblAdvMaxTitle.Content = Translator.Get("AdvMaxTitleLen");
            LblLanguage.Content = Translator.Get("AdvLanguage");
        }

        private void PopulateUiFromConfig()
        {
            TbTosuExePath.Text = _config.TosuExePath;
            TbTosuPort.Text = _config.TosuPort.ToString();
            TbOscHost.Text = _config.VRChatOscHost;
            TbOscPort.Text = _config.VRChatOscPort.ToString();
            TbTplPlaying1.Text = _config.Templates.PlayingLine1;
            TbTplPlaying2.Text = _config.Templates.PlayingLine2;
            TbTplPaused.Text = _config.Templates.PausedPrefix;
            TbTplReplay.Text = _config.Templates.WatchingReplay;
            TbTplReplayResult.Text = _config.Templates.ReplayResult;
            TbTplSongSelect.Text = _config.Templates.SongSelect;
            TbTplResult.Text = _config.Templates.ResultScreen;
            TbTplEditor.Text = _config.Templates.Editor;
            TbTplIdle.Text = _config.Templates.IdleText;
            CbUseUnicode.IsChecked = _config.UseUnicodeTitle;
            CbShowArtist.IsChecked = _config.ShowArtist;
            TbDispStar.Text = _config.StarDecimals.ToString();
            TbDispPp.Text = _config.PpDecimals.ToString();
            TbDispAcc.Text = _config.AccuracyDecimals.ToString();
            TbDispModeOsu.Text = _config.ModeNames.Osu;
            TbDispModeTaiko.Text = _config.ModeNames.Taiko;
            TbDispModeCatch.Text = _config.ModeNames.Catch;
            TbDispModeMania.Text = _config.ModeNames.Mania;
            TbAdvUpdate.Text = _config.UpdateIntervalMs.ToString();
            TbAdvResult.Text = _config.ResultScreenDurationS.ToString();
            TbAdvPause.Text = _config.PauseDetectionThresholdMs.ToString();
            TbAdvReconnect.Text = _config.ReconnectDelayMs.ToString();
            TbAdvMaxLen.Text = _config.MaxMessageLength.ToString();
            TbAdvMaxTitle.Text = _config.MaxTitleLength.ToString();

            var effectiveLang = Translator.EffectiveLanguage;
            foreach (System.Windows.Controls.ComboBoxItem item in CboLanguage.Items)
            {
                if (item.Tag?.ToString() == effectiveLang)
                {
                    CboLanguage.SelectedItem = item;
                    break;
                }
            }
        }

        private void SaveUiToConfig()
        {
            _config.TosuExePath = TbTosuExePath.Text;
            if (int.TryParse(TbTosuPort.Text, out int tp)) _config.TosuPort = tp;
            _config.VRChatOscHost = TbOscHost.Text;
            if (int.TryParse(TbOscPort.Text, out int op)) _config.VRChatOscPort = op;
            _config.Templates.PlayingLine1 = TbTplPlaying1.Text;
            _config.Templates.PlayingLine2 = TbTplPlaying2.Text;
            _config.Templates.PausedPrefix = TbTplPaused.Text;
            _config.Templates.WatchingReplay = TbTplReplay.Text;
            _config.Templates.ReplayResult = TbTplReplayResult.Text;
            _config.Templates.SongSelect = TbTplSongSelect.Text;
            _config.Templates.ResultScreen = TbTplResult.Text;
            _config.Templates.Editor = TbTplEditor.Text;
            _config.Templates.IdleText = TbTplIdle.Text;
            _config.UseUnicodeTitle = CbUseUnicode.IsChecked ?? false;
            _config.ShowArtist = CbShowArtist.IsChecked ?? false;
            if (int.TryParse(TbDispStar.Text, out int sd)) _config.StarDecimals = sd;
            if (int.TryParse(TbDispPp.Text, out int pd)) _config.PpDecimals = pd;
            if (int.TryParse(TbDispAcc.Text, out int ad)) _config.AccuracyDecimals = ad;
            _config.ModeNames.Osu = TbDispModeOsu.Text;
            _config.ModeNames.Taiko = TbDispModeTaiko.Text;
            _config.ModeNames.Catch = TbDispModeCatch.Text;
            _config.ModeNames.Mania = TbDispModeMania.Text;
            if (int.TryParse(TbAdvUpdate.Text, out int au)) _config.UpdateIntervalMs = au;
            if (int.TryParse(TbAdvResult.Text, out int ar)) _config.ResultScreenDurationS = ar;
            if (int.TryParse(TbAdvPause.Text, out int ap)) _config.PauseDetectionThresholdMs = ap;
            if (int.TryParse(TbAdvReconnect.Text, out int arec)) _config.ReconnectDelayMs = arec;
            if (int.TryParse(TbAdvMaxLen.Text, out int am)) _config.MaxMessageLength = am;
            if (int.TryParse(TbAdvMaxTitle.Text, out int mt)) _config.MaxTitleLength = mt;
            if (CboLanguage.SelectedItem is System.Windows.Controls.ComboBoxItem langItem && langItem.Tag is string langTag)
                _config.Language = langTag;
            ConfigManager.Save(_config);
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            SaveUiToConfig();
            MessageBox.Show(Translator.Get("ConfigSaved"), "OK", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnBrowseTosu_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog { Filter = "tosu|tosu.exe|All|*.*", Title = "Select tosu.exe" };
            if (dlg.ShowDialog() == true) TbTosuExePath.Text = dlg.FileName;
        }

        private void CboLanguage_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (_isInitializing) return;
            if (CboLanguage.SelectedItem is System.Windows.Controls.ComboBoxItem item && item.Tag is string tag)
            {
                _config.Language = tag;
                Translator.Language = tag;
                ApplyI18n();
                UpdateStatus();
                ConfigManager.Save(_config);
            }
        }

        private async void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            SaveUiToConfig();
            BtnStart.IsEnabled = false;
            BtnStopOsc.IsEnabled = true;
            BtnStop.IsEnabled = true;

            // If tosu is already running (e.g. after Stop OSC), only restart OSC
            if (_tosuClient != null && _tosuClient.IsConnected)
            {
                try { _oscSender = new VRChatOscSender(_config.VRChatOscHost, _config.VRChatOscPort); }
                catch (Exception ex) { MessageBox.Show($"OSC Error: {ex.Message}", "Error"); DoStop(); return; }

                _updateTimer.Interval = TimeSpan.FromMilliseconds(_config.UpdateIntervalMs);
                _updateTimer.Start();
                _currentGameState = GameState.Idle;
                UpdateStatus();
                return;
            }

            TxtStatus.Text = Translator.Get("StatusStartingTosu");
            _tosuProcess = new TosuProcessManager(_config.TosuExePath, _config.TosuHost, _config.TosuPort);

            string? err = _tosuProcess.Start(hidden: true);
            if (err != null)
            {
                MessageBox.Show(err, Translator.Get("TosuNotFound"), MessageBoxButton.OK, MessageBoxImage.Warning);
                DoStop(); return;
            }

            bool ready = await _tosuProcess.WaitForReady(10000);
            if (!ready)
            {
                // Silent launch failed — kill and retry with visible window, wait indefinitely
                _tosuProcess.Stop();
                err = _tosuProcess.Start(hidden: false);
                if (err != null)
                {
                    MessageBox.Show(err, Translator.Get("TosuNotFound"), MessageBoxButton.OK, MessageBoxImage.Warning);
                    DoStop(); return;
                }
                await _tosuProcess.WaitForReady(Timeout.Infinite);
            }

            try { _oscSender = new VRChatOscSender(_config.VRChatOscHost, _config.VRChatOscPort); }
            catch (Exception ex) { MessageBox.Show($"OSC Error: {ex.Message}", "Error"); DoStop(); return; }

            _tosuClient = new TosuWebSocketClient(_config.TosuHost, _config.TosuPort, _config.ReconnectDelayMs);
            _tosuClient.OnConnectionChanged += (c) => Dispatcher.Invoke(UpdateStatus);
            await _tosuClient.StartAsync();
            UpdateStatus();

            await _tosuClient.WaitForConnectionAsync(Timeout.Infinite);

            _updateTimer.Interval = TimeSpan.FromMilliseconds(_config.UpdateIntervalMs);
            _updateTimer.Start();
            UpdateStatus();
        }

        private void BtnStopOsc_Click(object? sender, RoutedEventArgs? e) => DoStopOsc();

        private void BtnStop_Click(object? sender, RoutedEventArgs? e) => DoStop();

        private void DoStop()
        {
            _updateTimer.Stop();
            _tosuClient?.Dispose(); _tosuClient = null;
            try { _oscSender?.ClearChatbox(); } catch { }
            _oscSender?.Dispose(); _oscSender = null;
            _tosuProcess?.Dispose(); _tosuProcess = null;
            BtnStart.IsEnabled = true;
            BtnStopOsc.IsEnabled = false;
            BtnStop.IsEnabled = false;
            _currentGameState = GameState.NotRunning;
            _wasWatchingReplay = false;
            _lastPlayFailed = false;
            UpdateStatus();
            TxtPreview.Text = "...";
        }

        private void DoStopOsc()
        {
            _updateTimer.Stop();
            try { _oscSender?.ClearChatbox(); } catch { }
            _oscSender?.Dispose(); _oscSender = null;
            BtnStart.IsEnabled = true;
            BtnStopOsc.IsEnabled = false;
            UpdateStatus();
            TxtPreview.Text = "...";
        }

        private void UpdateTimer_Tick(object? sender, EventArgs e)
        {
            if (_tosuClient == null || !_tosuClient.IsConnected || _tosuClient.LatestState == null)
            {
                _currentGameState = GameState.NotRunning;
                UpdateStatus(); return;
            }
            UpdateStatus();
            var state = _tosuClient.LatestState;
            var prevState = _currentGameState;
            DetermineGameState(state);

            // Force 0:00 on the first tick when entering Playing/WatchingReplay
            bool justStartedPlaying = (_currentGameState == GameState.Playing || _currentGameState == GameState.WatchingReplay)
                && prevState != GameState.Playing && prevState != GameState.WatchingReplay && prevState != GameState.Paused;

            string output = ChatboxFormatter.Format(state, _currentGameState, _config, justStartedPlaying);
            TxtPreview.Text = string.IsNullOrEmpty(output) ? "..." : output;
            if (_oscSender != null && !string.IsNullOrEmpty(output))
            {
                try { _oscSender.SendChatbox(output); } catch { }
            }
        }

        private void DetermineGameState(OsuState state)
        {
            int rawState = state.State?.Number ?? 0;

            // Forced result/replay-result hold
            if (_currentGameState == GameState.ResultScreen
                || _currentGameState == GameState.ReplayResultScreen
                || _currentGameState == GameState.FailedResultScreen)
            {
                bool holdExpired = (DateTime.Now - _resultScreenStartTime).TotalSeconds >= _config.ResultScreenDurationS;
                if (!holdExpired && rawState != 2) return;
            }

            if (rawState == 7)
            {
                if (_currentGameState != GameState.ResultScreen
                    && _currentGameState != GameState.ReplayResultScreen
                    && _currentGameState != GameState.FailedResultScreen)
                {
                    bool isFailedResult = !_wasWatchingReplay
                        && (_lastPlayFailed || string.Equals(state.ResultsScreen?.Rank, "F", StringComparison.OrdinalIgnoreCase));
                    _resultScreenStartTime = DateTime.Now;
                    _currentGameState = _wasWatchingReplay
                        ? GameState.ReplayResultScreen
                        : isFailedResult ? GameState.FailedResultScreen : GameState.ResultScreen;
                }
                return;
            }

            // 1 = Edit, 4 = SelectEdit
            if (rawState == 1 || rawState == 4) { _currentGameState = GameState.Editor; _wasWatchingReplay = false; _lastPlayFailed = false; return; }

            if (rawState == 5) { _currentGameState = GameState.SongSelect; _wasWatchingReplay = false; _lastPlayFailed = false; return; }

            if (rawState == 2)
            {
                string profileName = state.Profile?.Name ?? "";
                string playerName = state.Play?.PlayerName ?? "";
                bool hasFailed = state.Play?.Failed ?? false;
                int timeLive = state.Beatmap?.Time?.Live ?? 0;

                bool isReplay = false;
                if (!isReplay && !string.IsNullOrEmpty(playerName))
                {
                    if (string.IsNullOrEmpty(profileName))
                    {
                        // Guest mode fallback: If player is empty or "Guest", it's own play. Anything else is replay.
                        isReplay = !playerName.Equals("Guest", StringComparison.OrdinalIgnoreCase);
                    }
                    else
                    {
                        // Logged in fallback: If playing name differs from profile name, it's a replay.
                        isReplay = !playerName.Equals(profileName, StringComparison.OrdinalIgnoreCase);
                    }
                }

                _wasWatchingReplay = isReplay;
                if (!isReplay)
                {
                    // Treat failed as a latched state: tosu may only report it for a single tick.
                    _lastPlayFailed = _lastPlayFailed || hasFailed;

                    // A backwards time jump means a new attempt started, so the previous failed state can be cleared.
                    if (_lastPlayFailed && _lastTimeLive >= 0 && timeLive < _lastTimeLive)
                    {
                        _lastPlayFailed = false;
                    }
                }
                else
                {
                    _lastPlayFailed = false;
                }

                if (_lastPlayFailed && !isReplay)
                {
                    if (timeLive != _lastTimeLive)
                    {
                        _lastTimeLive = timeLive;
                        _lastTimeLiveChanged = DateTime.Now;
                    }
                    _currentGameState = GameState.Failed;
                    return;
                }

                if (timeLive != _lastTimeLive)
                {
                    _lastTimeLive = timeLive;
                    _lastTimeLiveChanged = DateTime.Now;
                    _currentGameState = isReplay ? GameState.WatchingReplay : GameState.Playing;
                }
                else if ((DateTime.Now - _lastTimeLiveChanged).TotalMilliseconds > _config.PauseDetectionThresholdMs)
                {
                    int lastObject = state.Beatmap?.Time?.LastObject ?? 0;
                    if (lastObject == 0 || timeLive < lastObject)
                    {
                        _currentGameState = GameState.Paused;
                    }
                }
                return;
            }

            _currentGameState = GameState.Idle;
            _wasWatchingReplay = false;
            _lastPlayFailed = false;
        }

        private void UpdateStatus()
        {
            if (_tosuClient == null)
                TxtStatus.Text = Translator.Get("StatusDisconnected");
            else if (_tosuClient.IsConnected)
                TxtStatus.Text = _oscSender != null
                    ? Translator.Get("StatusConnected")
                    : Translator.Get("StatusOscStopped");
            else
                TxtStatus.Text = Translator.Get("StatusTosuDisconnected");
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            SaveUiToConfig();
            DoStop();
            base.OnClosing(e);
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }
    }
}
