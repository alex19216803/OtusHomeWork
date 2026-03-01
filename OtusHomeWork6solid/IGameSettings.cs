// IGameSettings.cs - Интерфейс для настроек игры (ISP)
namespace GuessNumberGame
{
    public interface IGameSettings
    {
        int MinNumber { get; }
        int MaxNumber { get; }
        int MaxAttempts { get; }
        bool IsShowTargetNumber { get; }
    }
}