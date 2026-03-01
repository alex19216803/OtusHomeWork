// FileLogger.cs - Альтернативная реализация логирования (LSP, OCP)
namespace GuessNumberGame
{
    public class FileLogger : IGameLogger
    {
        private readonly string _logFilePath;

        public FileLogger(string logFilePath)
        {
            _logFilePath = logFilePath;
        }

        public void Log(string message)
        {
            File.AppendAllText(_logFilePath, $"[INFO] {DateTime.Now}: {message}{Environment.NewLine}");
        }

        public void LogError(string message)
        {
            File.AppendAllText(_logFilePath, $"[ERROR] {DateTime.Now}: {message}{Environment.NewLine}");
        }
    }
}