// IGameUI.cs - Интерфейс для пользовательского интерфейса (ISP)
namespace GuessNumberGame
{
    public interface IGameUI
    {
        void DisplayMessage(string message);
        void DisplayError(string message);
        int GetUserGuess();
        bool AskForReplay();
    }
}