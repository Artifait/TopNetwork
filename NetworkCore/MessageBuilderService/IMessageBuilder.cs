
using TopNetwork.Core;

namespace TopNetwork.MessageBuilderService
{
    public interface IRequest
    {
        string MessageType { get; }
    }
    public interface IResponse
    {
        string MessageType { get; }
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
