
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace TopNetwork.Core
{
    public class TopClient : IEquatable<TopClient>
    {
        private TcpClient _client;
        private NetworkStream _stream;
        private readonly ConcurrentQueue<Message> _messageQueue = new();
        private readonly SemaphoreSlim _streamSemaphore = new(1, 1);
        private readonly object _stateLock = new();

        private Action<Message>? _onMessageReceived;
        public event Action? OnDisconnected;

        public bool IsConnected => _client?.Connected ?? false;
        public bool IsInitialized { get; private set; }
        public EndPoint? RemoteEndPoint => _client?.Client.RemoteEndPoint;

        public Action<Message>? OnMessageReceived
        {
            get => _onMessageReceived;
            set
            {
                lock (_stateLock)
                {
                    _onMessageReceived = value;
                    if (_onMessageReceived != null)
                    {
                        Task.Run(ProcessQueuedMessages);
                    }
                }
            }
        }

        public TopClient() { }

        public TopClient(string ip, int port) => Initialize(ip, port);

        public TopClient(TcpClient client)
        {
            _client = client;
            _stream = client.GetStream();
            IsInitialized = true;
        }

        public void Initialize(string ip, int port)
        {
            if (IsInitialized)
                throw new InvalidOperationException("Client is already initialized.");

            _client = new TcpClient(ip, port);
            _stream = _client.GetStream();
            IsInitialized = true;
        }

        public async Task SendMessageAsync(Message message, CancellationToken cancellationToken = default)
        {
            if (!IsInitialized)
                throw new InvalidOperationException("Client is not initialized.");

            await _streamSemaphore.WaitAsync(cancellationToken);
            try
            {
                await DeliveryService.SendMessageAsync(_stream, message);
            }
            finally
            {
                _streamSemaphore.Release();
            }
        }

        public async Task StartListeningAsync(CancellationToken cancellationToken)
        {
            if (!IsInitialized)
                throw new InvalidOperationException("Client is not initialized.");

            try
            {
                while (!cancellationToken.IsCancellationRequested && IsConnected)
                {
                    var message = await DeliveryService.AcceptMessageAsync(this, cancellationToken);
                    EnqueueMessage(message);
                }
            }
            catch (OperationCanceledException)
            {
                // Normal operation on cancellation
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during message reception: {ex.Message}");
            }
            finally
            {
                Disconnect();
            }
        }

        public void Disconnect()
        {
            lock (_stateLock)
            {
                if (!IsConnected)
                    return;

                try
                {
                    _stream?.Close();
                    _client?.Close();
                    _stream?.Dispose();
                    _client?.Dispose();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error during disconnect: {ex.Message}");
                }
                finally
                {
                    OnDisconnected?.Invoke();
                    IsInitialized = false;
                }
            }
        }

        private void EnqueueMessage(Message message)
        {
            lock (_stateLock)
            {
                if (_onMessageReceived == null)
                {
                    _messageQueue.Enqueue(message);
                }
                else
                {
                    Task.Run(() => _onMessageReceived?.Invoke(message));
                }
            }
        }

        private async Task ProcessQueuedMessages()
        {
            while (_messageQueue.TryDequeue(out var message))
            {
                await Task.Run(() => _onMessageReceived?.Invoke(message));
            }
        }

        #region Equality Members

        public override int GetHashCode() => _client?.Client?.RemoteEndPoint?.ToString()?.GetHashCode() ?? 0;

        public override bool Equals(object? obj) => Equals(obj as TopClient);

        public bool Equals(TopClient? other) =>
            other != null &&
            IsInitialized == other.IsInitialized &&
            _client?.Client?.RemoteEndPoint?.Equals(other._client?.Client?.RemoteEndPoint) == true;

        public static bool operator ==(TopClient? left, TopClient? right) =>
            ReferenceEquals(left, right) || (left?.Equals(right) ?? false);

        public static bool operator !=(TopClient? left, TopClient? right) => !(left == right);

        #endregion
    }
}
