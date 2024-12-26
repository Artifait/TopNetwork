
using System.Net;
using TopNetwork.Core;
using TopNetwork.RequestResponse;

namespace ServerUltra
{
    public class TextSession : ClientSession
    {
        protected TextSession(TopClient client, RrServerHandlerBase messageHandler, ServiceRegistry context) : base(client, messageHandler, context)
        {
        }

        public static ClientSession SessionFactory(TopClient client, ServiceRegistry context)
        {
            
        }
    }
    internal class Program
    {
        static async Task Main()
        {
            string serverIp = "127.0.0.1";
            int port = 5335;

            RrServer server = new(new IPEndPoint(IPAddress.Parse(serverIp), port), )

            server.ClientConnected += client =>
            {
                Console.WriteLine($"Client connected: {client.RemoteEndPoint}");
            };

            server.ClientDisconnected += client =>
            {
                Console.WriteLine($"Client disconnected: {client.RemoteEndPoint}");
            };

            server.ClientRejected += async client =>
            {
                await Console.Out.WriteLineAsync($"Client rejected: {client.RemoteEndPoint}");
            };

            server.Init(IPAddress.Parse(serverIp), port);

            server.ServerHandlers.AddHandlerForMessageType("Text", async (client, message) =>
            {
                await Console.Out.WriteLineAsync($"Message received from {client.RemoteEndPoint}\n{message}");
                return new Message { Payload = "Response from server" };
            });
            server.ServerHandlers.SetDefaultHandler(async (client, message) => new Message { Payload = "Мы не смогли обработать ваш запрос..." });
            
            await server.Start();
            await Console.Out.WriteLineAsync("Чтоб остановить сервер, нажмите любую кнопку...");
            Console.ReadKey(true);

            await server.Stop();
        }
    }
}
