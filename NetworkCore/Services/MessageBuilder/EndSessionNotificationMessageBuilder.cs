
namespace TopNetwork.Services.MessageBuilder
{
    public class EndSessionNotificationData : IMsgSourceData
    {
        public string MessageType => "EndSessionNotification";
        public string Payload => "Время вашей сессии истекло, авторизируйтесь заного.";
    }

    public class EndSessionNotificationMessageBuilder
    {

    }
}
