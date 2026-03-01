// ConsoleUI.cs - Консольный пользовательский интерфейс (SRP, OCP)
namespace GuessNumberGame
{
    public class ConsoleUI : IGameUI
    {
        private readonly IGameLogger _logger;

        public ConsoleUI(IGameLogger logger)
        {
            _logger = logger;
        }

        public void DisplayMessage(string message)
        {
            Console.WriteLine(message);
            _logger.Log($"Displayed message: {message}");
        }

        public void DisplayError(string message)
        {
            Console.WriteLine($"Ошибка: {message}");
            _logger.LogError(message);
        }

        public int GetUserGuess()
        {
            Console.Write("Введите число: ");
            string input = Console.ReadLine();

            if (!int.TryParse(input, out int guess))
            {
                throw new FormatException("Введено некорректное число");
            }

            _logger.Log($"User guessed: {guess}");
            return guess;
        }

        public bool AskForReplay()
        {
            Console.Write("Хотите сыграть еще? (y/n): ");
            string response = Console.ReadLine()?.ToLower();
            return response == "y" || response == "yes";
        }
    }
}