using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace OsuOscVRC.Data
{
    public class TosuWebSocketClient : IDisposable
    {
        private ClientWebSocket? _webSocket;
        private CancellationTokenSource? _cts;
        private readonly string _uri;
        private readonly int _reconnectDelayMs;
        private TaskCompletionSource<bool>? _initialConnectionTcs;

        public OsuState? LatestState { get; private set; }
        public bool IsConnected => _webSocket?.State == WebSocketState.Open;

        public event Action<OsuState>? OnStateUpdated;
        public event Action<bool>? OnConnectionChanged;

        public TosuWebSocketClient(string host, int port, int reconnectDelayMs)
        {
            _uri = $"ws://{host}:{port}/websocket/v2";
            _reconnectDelayMs = reconnectDelayMs;
        }

        public Task StartAsync()
        {
            Stop();
            _cts = new CancellationTokenSource();
            _initialConnectionTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            _ = Task.Run(() => ReceiveLoopAsync(_cts.Token));
            return Task.CompletedTask;
        }

        public async Task<bool> WaitForConnectionAsync(int timeoutMs, CancellationToken cancellationToken = default)
        {
            if (IsConnected) return true;

            var initialConnectionTcs = _initialConnectionTcs;
            if (initialConnectionTcs == null) return false;

            if (timeoutMs == Timeout.Infinite)
            {
                return await initialConnectionTcs.Task;
            }

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var delayTask = Task.Delay(timeoutMs, timeoutCts.Token);
            var completedTask = await Task.WhenAny(initialConnectionTcs.Task, delayTask);

            if (completedTask != initialConnectionTcs.Task)
            {
                return false;
            }

            timeoutCts.Cancel();
            return await initialConnectionTcs.Task;
        }

        public void Stop()
        {
            _cts?.Cancel();

            if (_webSocket != null)
            {
                if (_webSocket.State == WebSocketState.Open)
                {
                    _webSocket.Abort();
                }
                _webSocket.Dispose();
                _webSocket = null;
            }

            _cts?.Dispose();
            _cts = null;
            _initialConnectionTcs?.TrySetResult(false);
            _initialConnectionTcs = null;

            OnConnectionChanged?.Invoke(false);
        }

        private async Task ReceiveLoopAsync(CancellationToken token)
        {
            bool hasConnectedOnce = false;

            while (!token.IsCancellationRequested)
            {
                try
                {
                    using var ws = new ClientWebSocket();
                    _webSocket = ws;
                    await ws.ConnectAsync(new Uri(_uri), token);
                    hasConnectedOnce = true;
                    _initialConnectionTcs?.TrySetResult(true);
                    OnConnectionChanged?.Invoke(true);

                    var buffer = new byte[1024 * 64];
                    using var ms = new MemoryStream();

                    while (ws.State == WebSocketState.Open && !token.IsCancellationRequested)
                    {
                        var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), token);
                        if (result.MessageType == WebSocketMessageType.Close)
                            break;

                        ms.Write(buffer, 0, result.Count);

                        if (result.EndOfMessage)
                        {
                            ms.Position = 0;
                            try
                            {
                                var state = JsonSerializer.Deserialize<OsuState>(ms);
                                if (state != null)
                                {
                                    LatestState = state;
                                    OnStateUpdated?.Invoke(state);
                                }
                            }
                            catch (JsonException) { /* ignore malformed JSON */ }

                            ms.SetLength(0);
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception)
                {
                    // connection error — will retry
                }
                finally
                {
                    _webSocket = null;
                    OnConnectionChanged?.Invoke(false);
                }

                if (!token.IsCancellationRequested)
                {
                    int reconnectDelayMs = hasConnectedOnce ? _reconnectDelayMs : Math.Min(_reconnectDelayMs, 500);
                    try { await Task.Delay(reconnectDelayMs, token); }
                    catch (OperationCanceledException) { break; }
                }
            }
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
