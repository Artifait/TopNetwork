
using System.Collections.Concurrent;

namespace TopNetwork.Core.RequestResponse
{
    public class RequestResponseClient
    {
        private readonly TopClient _topClient;
        private readonly RrClientHandlerBase _handler;
        private readonly ConcurrentDictionary<string, TaskCompletionSource<Message>> _pendingResponses = new();

        public event Func<Message, Task>? OnMessageReceived;

        public RequestResponseClient(TopClient topClient, RrClientHandlerBase handler)
        {
            _topClient = topClient ?? throw new ArgumentNullException(nameof(topClient));
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));

            _topClient.OnMessageReceived += HandleIncomingMessageAsync;
        }

        /// <param name="message"> Сообщение для отправки </param>
        /// <param name="cancellationToken"> токен отмены </param>
        /// <returns> Ответ от сервера => не желательно использовать синхроно </returns>
        /// <exception cref="ArgumentNullException"> если message == null</exception>
        public async Task<Message?> SendMessageAsync(Message message, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(message);

            string messageId = Guid.NewGuid().ToString();
            message.Headers["MessageId"] = messageId;

            var tcs = new TaskCompletionSource<Message>();
            _pendingResponses[messageId] = tcs;

            try
            {
                await _topClient.SendMessageAsync(message, cancellationToken);

                // Ожидание ответа или отмены
                using (cancellationToken.Register(() => tcs.TrySetCanceled()))
                {
                    return await tcs.Task;
                }
            }
            finally
            {
                _pendingResponses.TryRemove(messageId, out _);
            }
        }

        private async Task HandleIncomingMessageAsync(Message message)
        {
            if (message == null)
                return;

            // Проверьте, является ли сообщение ответом
            if (message.Headers.TryGetValue("ResponseTo", out var messageId) && _pendingResponses.TryRemove(messageId, out var tcs))
            {
                tcs.TrySetResult(message);
            }
            else
            {
                // Если нет подписчика или заголовка "ResponseTo", обработать его с помощью обработчика
                var response = await _handler.HandleMessage(message);
                if (response != null)
                {
                    await _topClient.SendMessageAsync(response);
                }

                // Уведомление глобальных слушателей
                if (OnMessageReceived != null)
                {
                    await OnMessageReceived.Invoke(message);
                }
            }
        }
    }
}
