
using TopNetwork.Core;
using TopNetwork.Services.MessageBuilder;

namespace Core.MessageBuilder
{
    public class DefaultRequestData : IMsgSourceData
    {
        public string Payload { get; set; } = string.Empty;

        public string MessageType => MsgType;
        public static string MsgType => "DefaultRequest";
    }

    public class DefaultRequestMessageBuilder : IMessageBuilder<DefaultRequestData>
    {
        private DefaultRequestData _data = new();

        public DefaultRequestMessageBuilder SetPayload(string payload)
        {
            _data.Payload = payload;
            return this;
        }

        public Message BuildMsg()
        {
            return new Message()
            {
                MessageType = _data.MessageType,
                Payload = _data.Payload
            };
        }

        public static DefaultRequestData Parse(Message msg)
        {
            if (msg.MessageType != DefaultRequestData.MsgType)
                throw new InvalidOperationException("Incorrect message type.");

            return new DefaultRequestData() { Payload = msg.Payload };
        }
    }
}
