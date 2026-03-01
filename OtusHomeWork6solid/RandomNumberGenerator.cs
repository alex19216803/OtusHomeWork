// RandomNumberGenerator.cs - Генератор случайных чисел (SRP, OCP)
namespace GuessNumberGame
{
    public class RandomNumberGenerator : INumberGenerator
    {
        private readonly Random _random = new Random();

        public int GenerateNumber(int min, int max)
        {
            return _random.Next(min, max + 1);
        }
    }
}