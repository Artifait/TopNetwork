
using TopNetwork.Core;

namespace ClientUltra
{
    internal class Program
    {
        static int countMsgSended = 0;
        static async Task Main()
        {
            string serverIp = "127.0.0.1";
            int port = 5335;
            CancellationTokenSource cts = new();

            TopClient client = new(serverIp, port);
            client.OnAcceptedMessage += OnMessageFromServer;
            _ = client.StartListen(cts.Token);
            Message msg = new()
            {
                MessageType = "Text"
            };

            while (true)
            {
                if (Console.ReadKey(true).KeyChar != 'a')
                {
                    msg.Payload = $"Сообщение №{countMsgSended++}";
                    await client.SendMessageAsync(msg);
                }
                else break;
            }
        }
        
        private static void OnMessageFromServer(Message msg)
        {
            Console.WriteLine("Получили сообщение от сервера.");
            Console.WriteLine(msg.ToString());
        }   
    }
}
