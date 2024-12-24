
using System.Collections.Concurrent;
using System.Net;
using TopNetwork.Core.RequestResponse;

namespace TopNetwork.Core
{
    public class ClientSession
    {
        private readonly TopClient _client;
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private readonly object _stateLock = new();
        private readonly object _conditionsLock = new();

        /// <summary> Обработка сообщения клиента -> Здесь ответ от сервера </summary>
        public event Action<ClientSession, Message>? OnMessageProcessed;
        public event Action<ClientSession>? OnSessionStarted;
        public event Action<ClientSession>? OnSessionClosed;    

        public SessionCloseConditionEvaluator ConditionEvaluator { get; set; } = new();
        public RrServerHandlerBase MessageHandlers { get; set; }
        public readonly ServiceRegistry ServerContext;

        public LogString? logger;
        public bool IsRunning { get; private set; }
        public bool IsClosed { get; private set; }
        public EndPoint? RemoteEndPoint => _client.RemoteEndPoint;
        public DateTime StartTime { get; private set; }
        public int ProcessedMessagesCountAll => ProcessedMessagesCountOfType.Values.Sum();
        public ConcurrentDictionary<string, int> ProcessedMessagesCountOfType { get; private set; }

        protected ClientSession(TopClient client, RrServerHandlerBase messageHandler, ServiceRegistry context)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            MessageHandlers = messageHandler ?? throw new ArgumentNullException(nameof(messageHandler));
            _client.OnMessageReceived += HandleMessageAsync;
            _client.OnDisconnected += HandleClientDisconnected;
            ServerContext = context;
        }

        public virtual async Task StartAsync()
        {
            if (IsRunning)
                throw new InvalidOperationException("Session is already running.");

            IsRunning = true;
            StartTime = DateTime.UtcNow;
            OnSessionStarted?.Invoke(this);

            try
            {
                await _client.StartListeningAsync(_cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                // Нормальное завершение
            }
            finally
            {
                CloseSession();
            }
        }

        public virtual void CloseSession()
        {
            lock (_stateLock)
            {
                if (IsClosed) return;
                IsClosed = true;
            }

            try
            {
                _client.Disconnect();
            }
            catch (Exception ex)
            {
                OnError($"[{RemoteEndPoint}]: Error while closing session - {ex.Message}");
            }
            finally
            {
                IsRunning = false;
                _cancellationTokenSource.Cancel();
                OnSessionClosed?.Invoke(this);
            }
        }

        private async Task HandleMessageAsync(Message message)
        {
            await CheckCloseConditions();

            if (message == null) return;

            try
            {
                if (ProcessedMessagesCountOfType.TryGetValue(message.MessageType, out int cnt))
                    cnt++;
                else
                    ProcessedMessagesCountOfType[message.MessageType] = 1;

                var response = await MessageHandlers.HandleMessage(_client, message);

                if (response != null)
                {
                    await _client.SendMessageAsync(response);
                    OnMessageProcessed?.Invoke(this, response);
                }
            }
            catch (Exception ex)
            {
                OnError($"[{RemoteEndPoint}]: Error processing message - {ex.Message}");
            }
        }

        private void HandleClientDisconnected()
        {
            OnError($"[{RemoteEndPoint}]: Client disconnected");
            CloseSession();
        }

        protected async Task CheckCloseConditions()
        {
            if(await ConditionEvaluator.ShouldCloseAsync(this))
                lock (_conditionsLock)
                    CloseSession();
        }

        protected virtual void OnError(string errorMessage)
        {
            logger?.Invoke(errorMessage);
        }
    }
}
