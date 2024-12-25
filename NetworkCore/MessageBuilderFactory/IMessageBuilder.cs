
using TopNetwork.Core;

namespace TopNetwork.MessageBuilderFactory
{
    public interface IRequest
    {
        string MessageType { get; }
        Message BuildMessage();
    }

    public interface IResponse
    {
        string MessageType { get; }
        Message BuildMessage();
    }

    public interface IMessageBuilder<TRequest, TResponse>
        where TRequest : IRequest
        where TResponse : IResponse
    {
        Message BuildRequest();
        Message BuildResponse();

        TRequest ParseRequest(Message msg);
        TResponse ParseResponse(Message msg);
    }
}
