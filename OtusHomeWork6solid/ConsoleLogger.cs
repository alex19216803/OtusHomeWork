// ConsoleLogger.cs - Логирование в консоль (LSP, OCP)
namespace GuessNumberGame
{
    public class ConsoleLogger : IGameLogger
    {
        public void Log(string message)
        {
            Console.WriteLine($"[INFO] {message}");
        }

        public void LogError(string message)
        {
            Console.WriteLine($"[ERROR] {message}");
        }
    }
}