
using System.Net;
using System.Threading.Channels;
using TopNetwork.Core;
using TopNetwork.RequestResponse;
using TopNetwork.Services.MessageBuilder;

namespace ServerUltra
{
    internal class Program
    {
        private static readonly RrServerHandlerBase _handlers = new RrServerHandlerBase()
            .AddHandlerForMessageType("Text", async (client, msg) => {
                if(msg.Payload == "5")
                    return new ErroreMessageBuilder().SetPayload("фуу 5").BuildMsg();

                return new Message()
                {
                    MessageType = "Response",
                    Payload = $"Response from server on: {msg.Payload}."
                };
            });

        public static ClientSession SessionFactory(TopClient client, ServiceRegistry context)
            => new ClientSession(client, _handlers, context);

        static async Task Main()
        {
            string serverIp = "127.0.0.1";
            int port = 5335;

            RrServer server = new(new IPEndPoint(IPAddress.Parse(serverIp), port), SessionFactory)
            {
                Logger = Console.WriteLine
            };

            _ = server.StartAsync();

            await Console.Out.WriteLineAsync("Чтоб остановить сервер, нажмите любую кнопку...");
            Console.ReadKey(true);

            await server.StopAsync();
        }
    }
}
