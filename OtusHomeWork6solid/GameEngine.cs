// GameEngine.cs - Основной движок игры (SRP, DIP)
namespace GuessNumberGame
{
    public class GameEngine
    {
        private readonly IGameSettings _settings;
        private readonly INumberGenerator _numberGenerator;
        private readonly IGameUI _ui;
        private readonly IGameLogger _logger;

        public GameEngine(
            IGameSettings settings,
            INumberGenerator numberGenerator,
            IGameUI ui,
            IGameLogger logger)
        {
            _settings = settings;
            _numberGenerator = numberGenerator;
            _ui = ui;
            _logger = logger;
        }

        public void Run()
        {
            bool playAgain = true;

            while (playAgain)
            {
                PlaySingleGame();
                playAgain = _ui.AskForReplay();
            }

            _ui.DisplayMessage("Спасибо за игру!");
        }

        private void PlaySingleGame()
        {
            int targetNumber = _numberGenerator.GenerateNumber(_settings.MinNumber, _settings.MaxNumber);
            int attempts = 0;
            bool guessed = false;

            if (_settings.IsShowTargetNumber) _logger.Log($"New game started. Target number: {targetNumber}");
            _ui.DisplayMessage($"Угадайте число от {_settings.MinNumber} до {_settings.MaxNumber}. У вас {_settings.MaxAttempts} попыток.");

            while (attempts < _settings.MaxAttempts && !guessed)
            {
                try
                {
                    int guess = _ui.GetUserGuess();
                    attempts++;

                    if (guess < _settings.MinNumber || guess > _settings.MaxNumber)
                    {
                        _ui.DisplayError($"Число должно быть в диапазоне от {_settings.MinNumber} до {_settings.MaxNumber}");
                        continue;
                    }

                    if (guess == targetNumber)
                    {
                        _ui.DisplayMessage($"Поздравляю! Вы угадали число {targetNumber} за {attempts} попыток!");
                        guessed = true;
                    }
                    else if (guess < targetNumber)
                    {
                        _ui.DisplayMessage("Загаданное число больше");
                    }
                    else
                    {
                        _ui.DisplayMessage("Загаданное число меньше");
                    }
                }
                catch (FormatException ex)
                {
                    _ui.DisplayError(ex.Message);
                }
            }

            if (!guessed)
            {
                _ui.DisplayMessage($"К сожалению, вы не угадали. Загаданное число было {targetNumber}");
            }
        }
    }
}