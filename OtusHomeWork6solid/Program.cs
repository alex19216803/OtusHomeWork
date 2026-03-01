using GuessNumberGame;

namespace OtusHomeWork6solid
{
    class Program
    {
        static void Main(string[] args)
        {
            // Создаем зависимости
            IGameSettings settings = new GameSettings(
                minNumber: 1,
                maxNumber: 100,
                maxAttempts: 7,
                isShowTargetNumber : false
            );

            IGameLogger logger = new ConsoleLogger(); // Можем легко заменить на FileLogger
                                                      // IGameLogger logger = new FileLogger("game_log.txt");

            INumberGenerator numberGenerator = new RandomNumberGenerator();
            IGameUI ui = new ConsoleUI(logger);

            // Создаем игру с внедренными зависимостями
            GameEngine game = new GameEngine(settings, numberGenerator, ui, logger);

            // Запускаем игру
            game.Run();
        }
    }
}
