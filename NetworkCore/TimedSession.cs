
namespace TopNetwork.Core
{
    public class TimedSession : ClientSession
    {
        private readonly TimeSpan _timeout;

        public TimedSession(
            TopClient client,
            LogString? logger,
            Func<TopClient, Task<Message?>>? builderDisconnectMessage,
            Func<TopClient, Message, Task<Message?>> handleMessage,
            TimeSpan timeout)
            : base(client, logger, builderDisconnectMessage, handleMessage)
        {
            _timeout = timeout;
        }

        protected override async void OnSessionStarted()
        {
            await Task.Delay(_timeout, CancellationToken);
            if (!CancellationToken.IsCancellationRequested)
            {
                await DisconnectAsync();
            }
        }

        protected override void OnSessionEnded()
        {
            // Можно добавить дополнительную логику при завершении сессии
        }
    }
}
