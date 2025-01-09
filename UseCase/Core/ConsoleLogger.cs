
using TopNetwork.RequestResponse;

namespace UseCase.Core
{
    public static class ConsoleLogger
    {
        private static SemaphoreSlim _consoleSemaphore = new(1, 1);
        public static ConsoleColor DefaultColor { get; set; } = ConsoleColor.White;

        public static async Task LogLine(string msg, ConsoleColor color = ConsoleColor.White)
            => await ConsoleLog(msg, Console.WriteLine, color);

        public static async Task Log(string msg, ConsoleColor color = ConsoleColor.White)
            => await ConsoleLog(msg, Console.Write, color);

        public static async Task ConsoleLog(string msg, LogString consoleLogger, ConsoleColor color = ConsoleColor.White)
        {
            await _consoleSemaphore.WaitAsync();
            try
            {
                Console.ForegroundColor = color;
                consoleLogger(msg);
            }
            finally { Console.ForegroundColor = DefaultColor; _consoleSemaphore.Release(); }
        }
    }
}
