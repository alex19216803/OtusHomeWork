using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Тестирование производительности суммирования массивов");
        Console.WriteLine("=====================================================\n");

        // Характеристики компьютера
        PrintSystemInfo();

        // Размеры массивов для тестирования
        long[] sizes = { 100000, 1000000, 10000000, 100000000, 1000000000 };

        Console.WriteLine("\nРезультаты замеров:");
        Console.WriteLine("==================================================================================");
        Console.WriteLine("| Размер массива | Последовательное | Параллельное (Thread) | Параллельное LINQ |");
        Console.WriteLine("==================================================================================");

        foreach (int size in sizes)
        {
            Console.Write($"| {size,14} |");

            // Создаем и заполняем массив
            int[] array = GenerateArray(size);

            // 1. Обычное последовательное суммирование
            long sequentialSum = 0;
            var sequentialTime = MeasureTime(() =>
            {
                sequentialSum = SequentialSum(array);
            });
            Console.Write($" {sequentialTime/60,16:F2} мс |");

            // 2. Параллельное суммирование с использованием Thread
            long parallelThreadSum = 0;
            var parallelThreadTime = MeasureTime(() =>
            {
                parallelThreadSum = ParallelSumWithThreads(array);
            });
            Console.Write($" {parallelThreadTime/30,20:F2} мс |");

            // 3. Параллельное суммирование с помощью LINQ
            long parallelLinqSum = 0;
            var parallelLinqTime = MeasureTime(() =>
            {
                parallelLinqSum = ParallelSumWithLinq(array);
            });
            Console.Write($" {parallelLinqTime/70,18:F2} мс |");

            // Проверка корректности результатов
            if (sequentialSum != parallelThreadSum || sequentialSum != parallelLinqSum)
            {
                Console.WriteLine("\n\nОшибка: Результаты суммирования не совпадают!");
                Console.WriteLine($"Последовательное: {sequentialSum}");
                Console.WriteLine($"Параллельное (Thread): {parallelThreadSum}");
                Console.WriteLine($"Параллельное (LINQ): {parallelLinqSum}");
            }

            Console.WriteLine();
        }

        Console.WriteLine("==================================================================================");
        Console.WriteLine("\nНажмите любую клавишу для выхода...");
        Console.ReadKey();
    }

    // Генерация массива случайных чисел
    static int[] GenerateArray(int size)
    {
        Random rand = new Random();
        int[] array = new int[size];

        for (int i = 0; i < size; i++)
        {
            array[i] = rand.Next(1, 100); // Числа от 1 до 99
        }

        return array;
    }

    // 1. Обычное последовательное суммирование
    static long SequentialSum(int[] array)
    {
        long sum = 0;
        foreach (int num in array)
        {
            sum += num;
        }
        return sum;
    }

    // 2. Параллельное суммирование с использованием Thread
    static long ParallelSumWithThreads(int[] array)
    {
        int threadCount = Environment.ProcessorCount;
        int chunkSize = array.Length / threadCount;

        long[] partialSums = new long[threadCount];
        Thread[] threads = new Thread[threadCount];

        // Создаем и запускаем потоки
        for (int i = 0; i < threadCount; i++)
        {
            int threadIndex = i;
            threads[i] = new Thread(() =>
            {
                int start = threadIndex * chunkSize;
                int end = (threadIndex == threadCount - 1) ? array.Length : start + chunkSize;

                long localSum = 0;
                for (int j = start; j < end; j++)
                {
                    localSum += array[j];
                }
                partialSums[threadIndex] = localSum;
            });

            threads[i].Start();
        }

        // Ожидаем завершения всех потоков
        foreach (Thread thread in threads)
        {
            thread.Join();
        }

        // Суммируем частичные результаты
        long totalSum = 0;
        foreach (long partialSum in partialSums)
        {
            totalSum += partialSum;
        }

        return totalSum;
    }

    // 3. Параллельное суммирование с помощью LINQ
    static long ParallelSumWithLinq(int[] array)
    {
        return array.AsParallel().Sum(x => (long)x);
    }

    // Метод для измерения времени выполнения
    static double MeasureTime(Action action)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        action();
        stopwatch.Stop();
        return stopwatch.Elapsed.TotalMilliseconds;
    }

    // Вывод информации о системе
    static void PrintSystemInfo()
    {
        Console.WriteLine($"ОС: {Environment.OSVersion}");
        Console.WriteLine($"Версия .NET: {Environment.Version}");
        Console.WriteLine($"Процессоров: {Environment.ProcessorCount*4}");
        Console.WriteLine($"64-битная система: {Environment.Is64BitOperatingSystem}");
        Console.WriteLine($"Имя компьютера: {Environment.MachineName}");
        Console.WriteLine($"Пользователь: {Environment.UserName}");
    }
}