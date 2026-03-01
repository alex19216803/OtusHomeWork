// INumberGenerator.cs - Интерфейс для генерации чисел (DIP)
namespace GuessNumberGame
{
    public interface INumberGenerator
    {
        int GenerateNumber(int min, int max);
    }
}