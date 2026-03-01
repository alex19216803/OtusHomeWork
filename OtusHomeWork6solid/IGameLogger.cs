// IGameLogger.cs - Интерфейс для логирования (ISP, DIP)
namespace GuessNumberGame
{
    public interface IGameLogger
    {
        void Log(string message);
        void LogError(string message);
    }
}