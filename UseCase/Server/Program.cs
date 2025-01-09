
using Core.MessageBuilder;
using System.Net;
using TopNetwork.Core;
using TopNetwork.RequestResponse;
using TopNetwork.Services.MessageBuilder;
using UseCase.Core;

namespace UseCase.Server
{
    internal class Program
    {
        // Регистрация всех фабрик для типов сообщений отправляемых сервером 
        private static MessageBuilderService _msgBuilderService = new MessageBuilderService()
                    .Register(() => new ErroreMessageBuilder())             // Ошибка
                    .Register(() => new DefaultResponseMessageBuilder());   // Ответ

        private static readonly RrServerHandlerBase _handlers = new RrServerHandlerBase()
            .AddHandlerForMessageType(DefaultRequestData.MsgType, async (client, msg, context) => {
                var request = DefaultRequestMessageBuilder.Parse(msg);

                if(request.Payload == "5")
                {
                    return context
                        .Get<MessageBuilderService>()!
                        .BuildMessage<ErroreMessageBuilder, ErroreData>(
                            builder => builder.SetPayload("ABAAABA"));
                }

                return context
                        .Get<MessageBuilderService>()!
                        .BuildMessage<DefaultResponseMessageBuilder, DefaultResponseData>(
                            builder => builder.SetPayload($"Вы отправили: {request.Payload}."));
            });

        public static async Task<ClientSession> SessionFactory(TopClient client, ServiceRegistry context, LogString? logger)
            => new ClientSession(client, _handlers, context) { logger = logger };

        static async Task Main()
        {
            string serverIp = "127.0.0.1";
            int port = 5335;

            IPEndPoint endPoint = new(IPAddress.Parse(serverIp), port);
            RrServer server = new RrServer().SetEndPoint(endPoint);

            server.Logger = async msg => await ConsoleLogger.LogLine(msg, ConsoleColor.White);

            server.SetSessionFactory(SessionFactory);
            server.RegisterService(_msgBuilderService);

            _ = server.StartAsync();
            await ConsoleLogger.LogLine("Чтоб остановить сервер, нажмите любую кнопку...", ConsoleColor.Yellow);
            Console.ReadKey(true);

            await server.StopAsync();
        }
    }
}
