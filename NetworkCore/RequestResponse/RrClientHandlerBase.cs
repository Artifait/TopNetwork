
using System.Collections.Concurrent;

namespace TopNetwork.Core.RequestResponse
{
    public class RrClientHandlerBase
    {
        public ConcurrentDictionary<string, Func<Message, Task<Message?>>> HandlerOfMessageType { get; private set; }

        public RrClientHandlerBase()
        {
            HandlerOfMessageType = [];
        }

        public async Task<Message?> HandleMessage(Message msg)
        {
            string msgType = msg.MessageType;

            if (HandlerOfMessageType.TryGetValue(msgType, out var func))
                return await func(msg);

            return null;
        }

        public void AddHandlerForMessageType(string type, Func<Message, Task<Message?>> handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            if (HandlerOfMessageType.ContainsKey(type))
                throw new ArgumentException("Данный тип уже иммет свой обрабтчик");

            if (string.IsNullOrWhiteSpace(type))
                throw new ArgumentException("Тип сообщения не может быть пустым.");

            HandlerOfMessageType[type] = handler;
        }

        public void Clear() => HandlerOfMessageType.Clear();
    }
}
