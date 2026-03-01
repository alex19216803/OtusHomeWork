// GameSettings.cs - Класс для хранения настроек игры (SRP)
namespace GuessNumberGame
{
    public class GameSettings : IGameSettings
    {
        public int MinNumber { get; }
        public int MaxNumber { get; }
        public int MaxAttempts { get; }
        public bool IsShowTargetNumber { get; }

        public GameSettings(int minNumber, int maxNumber, int maxAttempts, bool isShowTargetNumber)
        {
            MinNumber = minNumber;
            MaxNumber = maxNumber;
            MaxAttempts = maxAttempts;
            IsShowTargetNumber = isShowTargetNumber;
        }
    }
}