
namespace TopNetwork.Core
{
    public abstract class ClientSession
    {
        private readonly TopClient _client;
        private readonly LogString? _logger;
        private readonly Func<TopClient, Task<Message?>>? _builderDisconnectMessage;
        private readonly Func<TopClient, Message, Task<Message?>> _handleMessage;
        private readonly CancellationTokenSource _cancellationTokenSource = new();

        protected CancellationToken CancellationToken => _cancellationTokenSource.Token;

        public bool IsRunning { get; private set; }

        protected ClientSession(
            TopClient client,
            LogString? logger,
            Func<TopClient, Task<Message?>>? builderDisconnectMessage,
            Func<TopClient, Message, Task<Message?>> handleMessage)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _logger = logger;
            _builderDisconnectMessage = builderDisconnectMessage;
            _handleMessage = handleMessage ?? throw new ArgumentNullException(nameof(handleMessage));
        }

        public async Task StartAsync()
        {
            if (IsRunning)
                throw new InvalidOperationException("Session is already running.");

            IsRunning = true;

            try
            {
                _logger?.Invoke($"Session started for client: {_client.RemoteEndPoint}");

                _client.OnMessageReceived += async message =>
                {
                    if (message == null)
                    {
                        _logger?.Invoke($"Client {_client.RemoteEndPoint} sent a null message.");
                        return;
                    }

                    try
                    {
                        var response = await _handleMessage(_client, message);
                        if (response != null)
                        {
                            await _client.SendMessageAsync(response);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger?.Invoke($"Error handling message from {_client.RemoteEndPoint}: {ex.Message}");
                    }
                };

                await _client.StartListen(async message =>
                {
                    if (message == null)
                    {
                        _logger?.Invoke($"Client {_client.RemoteEndPoint} sent a null message.");
                        return;
                    }

                    try
                    {
                        var response = await _handleMessage(_client, message);
                        if (response != null)
                        {
                            await _client.SendMessageAsync(response);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger?.Invoke($"Error handling message from {_client.RemoteEndPoint}: {ex.Message}");
                    }
                }, CancellationToken);
            }
            catch (OperationCanceledException)
            {
                _logger?.Invoke($"Session cancelled for client: {_client.RemoteEndPoint}");
            }
            finally
            {
                await DisconnectAsync();
            }
        }

        public async Task DisconnectAsync()
        {
            if (!IsRunning)
                return;

            IsRunning = false;
            _cancellationTokenSource.Cancel();

            try
            {
                if (_builderDisconnectMessage != null)
                {
                    var disconnectMessage = await _builderDisconnectMessage(_client);
                    if (disconnectMessage != null)
                    {
                        await _client.SendMessageAsync(disconnectMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.Invoke($"Error sending disconnect message for {_client.RemoteEndPoint}: {ex.Message}");
            }
            finally
            {
                _client.Disconnect();
                _cancellationTokenSource.Dispose();
                _logger?.Invoke($"Session closed for client: {_client.RemoteEndPoint}");
            }
        }

        protected abstract void OnSessionStarted();
        protected abstract void OnSessionEnded();
    }
}
