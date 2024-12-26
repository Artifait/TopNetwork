
using TopNetwork.Core;
using TopNetwork.RequestResponse;

namespace ClientUltra
{
    internal class Program
    {
        static int countMsgSended = 0;
        static async Task Main()
        {
            string serverIp = "127.0.0.1";
            int port = 5335;

            TopClient _client = new();
            try
            {
                _client.Connect(new System.Net.Sockets.TcpClient(serverIp, port));
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync("Сервер не пускает вот что пишит:");
                Console.WriteLine(ex.ToString());
            }

            RrClientHandlerBase _handlers = new();
            _handlers.AddHandlerForMessageType("Errore",
                async (Message msg) => {
                    Console.WriteLine(msg.ToString());
                    return null;
                });
            _handlers.AddHandlerForMessageType("Response",
                async (Message msg) =>
                {
                    Console.WriteLine(msg.ToString());
                    return null;
                });


            RrClient client = new(_client, _handlers);
            _ = _client.StartListeningAsync();
            Message msg = new()
            {
                MessageType = "Text"
            };

            while (true)
            {
                if (Console.ReadKey(true).KeyChar != 'a')
                {
                    msg.Payload = $"{countMsgSended++}";
                    await Console.Out.WriteLineAsync($"Отправка сообщения №{countMsgSended}");
                    var response = await client.SendMessageWithResponseAsync(msg);
                    Console.WriteLine($"Ответ от сервера:\n{response}");
                }
                else break;
            }
        }
    }
}
