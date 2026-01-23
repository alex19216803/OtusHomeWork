using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

class Program
{
    public static async Task Main(string[] args)
    {
        // Генерация файлов
        const int N = 3;
        Stopwatch stopwatch = Stopwatch.StartNew();

        Console.WriteLine($"Loader started with process Id {Process.GetCurrentProcess().Id}...");

        IList<string> fileList = new List<string>();

        for (int i = 0; i < N; i++)
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"customers{i}.xml");
            fileList.Add(path);
            GenerateCustomersDataFile(path);
        }

        // Запуск загрузки файлов и подсчета пробелов в них
        Program program = new Program();

        int count = await program.ReadDataFromFolderAndCountSpacesAsync(
            AppDomain.CurrentDomain.BaseDirectory,
            SearchOption.AllDirectories
        );

        Console.WriteLine($"\nОбщее количество пробелов: {count}");
        stopwatch.Stop();
        Console.WriteLine($"Время выполнения, мс: {stopwatch.ElapsedMilliseconds}");
        Console.WriteLine("Нажмите любую клавишу для выхода...");
        Console.ReadKey();
    }

    private async Task<int> ReadDataFromFolderAndCountSpacesAsync(string folder, SearchOption searchOption)
    {
        if (!Directory.Exists(folder))
        {
            throw new DirectoryNotFoundException($"Каталог не найден: {folder}");
        }

        string[] files = Directory.GetFiles(
            folder,
            "*.xml",
            searchOption
        );

        IList<string> fileList = new List<string>();
        foreach (string file in files)
        {
            fileList.Add(file);
        }

        int spaceCount = 0;

        try
        {
            spaceCount = await ProcessFilesAsync(fileList);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }

        return spaceCount;
    }

    public static async Task<int> ProcessFilesAsync(IList<string> filePaths)
    {
        if (filePaths == null || filePaths.Count == 0)
        {
            Console.WriteLine("Список файлов пуст");
            return 0;
        }

        var tasks = new List<Task<(DataLoader Loader, string FilePath)>>();

        for (int i = 0; i < filePaths.Count; i++)
        {
            string filePath = filePaths[i];
            int fileIndex = i + 1;

            Task<(DataLoader, string)> task = Task.Run(async () =>
            {
                Console.WriteLine($"Начата загрузка файла {fileIndex}: {filePath}");
                var loader = new DataLoader();
                await loader.LoadDataAsync(filePath);
                Console.WriteLine($"Завершена загрузка файла {fileIndex}: {filePath}");
                return (loader, filePath);
            });

            tasks.Add(task);
        }

        Console.WriteLine("Ожидание завершения всех задач...");

        var results = await Task.WhenAll(tasks);

        Console.WriteLine("\nРезультаты подсчета пробелов:");

        int totalSpaceCounter = 0;

        foreach (var result in results)
        {
            int spaceCount = result.Loader.GetSpaceCount();
            Console.WriteLine($"{result.FilePath}: {spaceCount} пробелов");
            totalSpaceCounter += spaceCount;
        }

        return totalSpaceCounter;
    }

    static void GenerateCustomersDataFile(string dataFilePath)
    {
        var xmlGenerator = new Otus.Teaching.Concurrency.Import.DataGenerator.Generators.XmlGenerator(dataFilePath, 1000);
        xmlGenerator.Generate();
    }
}

public interface IDataLoader
{
    Task LoadDataAsync(string path);
    string GetData();
    int GetSpaceCount();
}

public class DataLoader : IDataLoader
{
    private string _data;

    public async Task LoadDataAsync(string path)
    {
        _data = await File.ReadAllTextAsync(path);
    }

    public string GetData()
    {
        return _data;
    }

    public int GetSpaceCount()
    {
        return _data?.Count(c => c == ' ') ?? 0;
    }
}


