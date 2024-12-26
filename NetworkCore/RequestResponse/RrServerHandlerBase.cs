
using System.Collections.Concurrent;
using TopNetwork.Core;

namespace TopNetwork.RequestResponse
{
    public class RrServerHandlerBase
    {
        public ConcurrentDictionary<string, Func<TopClient, Message, Task<Message?>>> HandlerOfMessageType { get; private set; }
        public Func<TopClient, Message, Task<Message?>> DefaultHandler { get; private set; }

        public RrServerHandlerBase()
        {
            DefaultHandler = DefaultHandlerRealization;
            HandlerOfMessageType = [];
        }

        public async Task<Message?> HandleMessage(TopClient client, Message msg)
        {
            string msgType = msg.MessageType;

            if (HandlerOfMessageType.TryGetValue(msgType, out var func))
                return await func(client, msg);

            return await DefaultHandler(client, msg);
        }

        public RrServerHandlerBase AddHandlerForMessageType(string type, Func<TopClient, Message, Task<Message?>> handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            if (HandlerOfMessageType.ContainsKey(type))
                throw new ArgumentException("Данный тип уже иммет свой обрабтчик");

            if (string.IsNullOrWhiteSpace(type))
                throw new ArgumentException("Тип сообщения не может быть пустым.");

            HandlerOfMessageType[type] = handler;
            return this;
        }

        public void Clear() => HandlerOfMessageType.Clear();

        /// <summary>
        /// Установить обработчик для <see cref="Message"/> у которого тип не задан
        /// </summary>
        public void SetDefaultHandler(Func<TopClient, Message, Task<Message?>> handler) => DefaultHandler = handler;
        protected virtual async Task<Message?> DefaultHandlerRealization(TopClient client, Message msg) => null;
    }
}
