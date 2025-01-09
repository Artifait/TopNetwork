
using TopNetwork.Core;
using TopNetwork.Services.MessageBuilder;

namespace Core.MessageBuilder
{
    public class DefaultResponseData : IMsgSourceData
    {
        public string Payload { get; set; } = string.Empty;

        public string MessageType => MsgType;
        public static string MsgType => "DefaultResponse";
    }

    public class DefaultResponseMessageBuilder : IMessageBuilder<DefaultResponseData>
    {
        private DefaultResponseData _data = new();

        public DefaultResponseMessageBuilder SetPayload(string payload)
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

        public static DefaultResponseData Parse(Message msg)
        {
            if (msg.MessageType != DefaultResponseData.MsgType)
                throw new InvalidOperationException("Incorrect message type.");

            return new DefaultResponseData() { Payload = msg.Payload };
        }
    }
}
