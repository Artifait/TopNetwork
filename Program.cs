
using System.Net;
using TopNetwork.Core;
using Newtonsoft.Json.Linq;

namespace ServerUltra
{
    internal class Program
    {

        public static class CurrencyConverter
        {
            public enum CurrencyType
            {
                USD,
                RUB,
                EUR
            }

            public static async Task<double?> GetExchangeRate(CurrencyType fromCurrency, CurrencyType toCurrency)
            {
                using var httpClient = new HttpClient();

                try
                {
                    string apiUrl = "https://api.exchangerate-api.com/v4/latest/" + Enum.GetName(typeof(CurrencyType), fromCurrency);  // Бесплатный API
                    string response = await httpClient.GetStringAsync(apiUrl);
                    JObject data = JObject.Parse(response);
                    return data["rates"]?[Enum.GetName(typeof(CurrencyType), toCurrency)]?.ToObject<double>();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при получении курса: {ex.Message}");
                    return null;
                }
            }
            public static async Task<double?> GetExchangeRate(string fromCurrency, string toCurrency)
            {
                using var httpClient = new HttpClient();

                try
                {
                    string apiUrl = "https://api.exchangerate-api.com/v4/latest/" + fromCurrency;  // Бесплатный API
                    string response = await httpClient.GetStringAsync(apiUrl);
                    JObject data = JObject.Parse(response);
                    return data["rates"]?[toCurrency]?.ToObject<double>();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при получении курса: {ex.Message}");
                    return null;
                }
            }
        }


        static async Task Main()
        {
            string serverIp = "127.0.0.1";
            int port = 5335;

            var server = new RequestResponseServer
            {
                ShouldAcceptClient = async client =>
                {
                    await Console.Out.WriteLineAsync("Checking if client should be accepted...");
                    return client != null;
                }
            };

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
