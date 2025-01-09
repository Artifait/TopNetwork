
using Core.MessageBuilder;
using TopNetwork.RequestResponse;
using TopNetwork.Services.MessageBuilder;
using UseCase.Core;

namespace Client
{
    internal class Program
    {
        private static readonly string _serverIp = "127.0.0.1";
        private static readonly int _port = 5335;
        private static RrClient _client = null!;
        private static string _menuMessage =
            "==== Меню клиента ====\n" +
            "1. Отправить сообщение\n" +
            "2. Проверить состояние подключения\n" +
            "3. Переподключиться\n" +
            "4. Выйти\n" +
            "Выберите действие: ";

        // Регистрация всех фабрик для типов сообщений отправляемых клиентом 
        private static MessageBuilderService _msgBuilder = new MessageBuilderService()
            .Register(() => new DefaultRequestMessageBuilder());

        // Регистрация обработчиков для конкретных типов сообщений от сервера
        private static RrClientHandlerBase _handlerBase = new RrClientHandlerBase()
            .AddHandlerForMessageType(DefaultResponseData.MsgType, async msg =>
            {
                var response = DefaultResponseMessageBuilder.Parse(msg);
                await ConsoleLogger.LogLine($"[Server.Response]: {response.Payload}", ConsoleColor.Green);
                return null;
            })
            .AddHandlerForMessageType(ErroreData.MsgType, async msg =>
            {

                var erroreData = ErroreMessageBuilder.Parse(msg);
                await ConsoleLogger.LogLine($"[Server.Response]: Errore - {msg}", ConsoleColor.Red);
                return null;
            });

        static async Task Main(string[] args)
        {
            try
            {
                await ConsoleLogger.LogLine("Подключение к серверу...");

                _client = new RrClient(_handlerBase).Connect(_serverIp, _port);
                _ = _client.StartListening();

                await ConsoleLogger.LogLine($"Успешно подключено к серверу: {_serverIp}:{_port}", ConsoleColor.Green);

                // Основной цикл интерфейса
                while (true)
                {
                    Console.Clear();
                    await ConsoleLogger.LogLine(_menuMessage);

                    var input = Console.ReadLine();

                    switch (input)
                    {
                        case "1":
                            await ConsoleLogger.LogLine("Введите текст сообщения: ");
                            var message = Console.ReadLine();
                            if (!string.IsNullOrEmpty(message)) {
                                await SendMessage(message);
                            }
                            else {
                                await ConsoleLogger.LogLine("Сообщение не может быть пустым.", ConsoleColor.Red);
                            }
                            break;

                        case "2":
                            await ConsoleLogger.Log("Состояние подключения: ");
                            await ConsoleLogger.LogLine(_client.IsConnected ? "Подключено" : "Отключено", _client.IsConnected ? ConsoleColor.Green : ConsoleColor.Red);
                            break;

                        case "3":
                            try {
                                _client.Connect(_serverIp, _port);
                                await ConsoleLogger.LogLine($"Успешно подключено к серверу: {_serverIp}:{_port}", ConsoleColor.Green);
                            }
                            catch (Exception ex) {
                                await ConsoleLogger.LogLine("[Errore]: {ex.Message}", ConsoleColor.Red);
                            }
                            break;
                        case "4":
                            // Вот так выглядит настоящий костыль(так записано, чтоб не повторять логику с _consoleSemaphore)
                            await ConsoleLogger.ConsoleLog(string.Empty, str => Console.Clear());
                            await ConsoleLogger.LogLine("Завершение работы...");
                            _client.Disconnect();
                            return;

                        default:
                            await ConsoleLogger.LogLine("Неверный ввод. Попробуйте снова.", ConsoleColor.Yellow);
                            break;
                    }
                    Console.ReadLine();
                }
            }
            catch (Exception ex)
            {
                await ConsoleLogger.LogLine($"[Ошибка]: {ex.Message}", ConsoleColor.Red);
            }
        }

        public static async Task SendMessage(string payload)
        {
            try
            {
                var request = _msgBuilder.BuildMessage<DefaultRequestMessageBuilder, DefaultRequestData>(
                    builder => builder.SetPayload(payload)
                );

                await _client.SendMessageWithoutResponseAsync(request);
                await ConsoleLogger.LogLine($"[Успех]: Сообщение \"{payload}\" отправлено.", ConsoleColor.Green);
            }
            catch (Exception ex)
            {
                await ConsoleLogger.LogLine($"[Ошибка при отправке]: {ex.Message}", ConsoleColor.Red);
            }
        }
    }
}
