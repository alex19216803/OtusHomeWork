using System;
using System.Collections.Generic;

public interface IMyCloneable<T>
{
    T MyClone();
}

// Базовый класс "Комплектующее"
public class Component : IMyCloneable<Component>, ICloneable
{
    public string Manufacturer { get; set; }
    public string Model { get; set; }
    public decimal Price { get; set; }
    public int PowerConsumption { get; set; } 

    public Component(string manufacturer, string model, decimal price, int powerConsumption)
    {
        Manufacturer = manufacturer;
        Model = model;
        Price = price;
        PowerConsumption = powerConsumption;
    }

    public virtual Component MyClone()
    {
        return new Component(Manufacturer, Model, Price, PowerConsumption);
    }

    public object Clone()
    {
        return MyClone();
    }

    public override string ToString()
    {
        return $"Component: {Manufacturer} {Model}, Price: {Price:C}, Power: {PowerConsumption}W";
    }
}

public class Processor : Component, IMyCloneable<Processor>, ICloneable
{
    public double ClockSpeed { get; set; } 
    public int Cores { get; set; }
    public int Threads { get; set; }
    public string Socket { get; set; }

    public Processor(string manufacturer, string model, decimal price, int powerConsumption,
                    double clockSpeed, int cores, int threads, string socket)
        : base(manufacturer, model, price, powerConsumption)
    {
        ClockSpeed = clockSpeed;
        Cores = cores;
        Threads = threads;
        Socket = socket;
    }

    public override Component MyClone()
    {
        return new Processor(Manufacturer, Model, Price, PowerConsumption,
                           ClockSpeed, Cores, Threads, Socket);
    }

    Processor IMyCloneable<Processor>.MyClone()
    {
        return new Processor(Manufacturer, Model, Price, PowerConsumption,
                           ClockSpeed, Cores, Threads, Socket);
    }

    public override string ToString()
    {
        return $"Processor: {Manufacturer} {Model}, {ClockSpeed}GHz, {Cores}C/{Threads}T, " +
               $"Socket: {Socket}, Price: {Price:C}, {PowerConsumption}W";
    }
}

public class GraphicsCard : Component, IMyCloneable<GraphicsCard>, ICloneable
{
    public int MemoryGB { get; set; }
    public string MemoryType { get; set; }
    public double CoreClock { get; set; } 
    public int CudaCores { get; set; }

    public GraphicsCard(string manufacturer, string model, decimal price, int powerConsumption,
                       int memoryGB, string memoryType, double coreClock, int cudaCores)
        : base(manufacturer, model, price, powerConsumption)
    {
        MemoryGB = memoryGB;
        MemoryType = memoryType;
        CoreClock = coreClock;
        CudaCores = cudaCores;
    }

    public override Component MyClone()
    {
        return new GraphicsCard(Manufacturer, Model, Price, PowerConsumption,
                              MemoryGB, MemoryType, CoreClock, CudaCores);
    }

    GraphicsCard IMyCloneable<GraphicsCard>.MyClone()
    {
        return new GraphicsCard(Manufacturer, Model, Price, PowerConsumption,
                              MemoryGB, MemoryType, CoreClock, CudaCores);
    }

    public override string ToString()
    {
        return $"Graphics Card: {Manufacturer} {Model}, {MemoryGB}GB {MemoryType}, " +
               $"{CoreClock}MHz, {CudaCores} CUDA cores, Price: {Price:C}, {PowerConsumption}W";
    }
}

public class Motherboard : Component, IMyCloneable<Motherboard>, ICloneable
{
    public string FormFactor { get; set; }
    public string Chipset { get; set; }
    public string Socket { get; set; }
    public int RamSlots { get; set; }
    public int PcieSlots { get; set; }

    public Motherboard(string manufacturer, string model, decimal price, int powerConsumption,
                      string formFactor, string chipset, string socket, int ramSlots, int pcieSlots)
        : base(manufacturer, model, price, powerConsumption)
    {
        FormFactor = formFactor;
        Chipset = chipset;
        Socket = socket;
        RamSlots = ramSlots;
        PcieSlots = pcieSlots;
    }

    public override Component MyClone()
    {
        return new Motherboard(Manufacturer, Model, Price, PowerConsumption,
                             FormFactor, Chipset, Socket, RamSlots, PcieSlots);
    }

    Motherboard IMyCloneable<Motherboard>.MyClone()
    {
        return new Motherboard(Manufacturer, Model, Price, PowerConsumption,
                             FormFactor, Chipset, Socket, RamSlots, PcieSlots);
    }

    public override string ToString()
    {
        return $"Motherboard: {Manufacturer} {Model}, {FormFactor}, {Chipset}, " +
               $"Socket: {Socket}, RAM slots: {RamSlots}, PCIe slots: {PcieSlots}, Price: {Price:C}";
    }
}

public class GamingGraphicsCard : GraphicsCard, IMyCloneable<GamingGraphicsCard>, ICloneable
{
    public bool HasRGB { get; set; }
    public int FansCount { get; set; }
    public bool Overclocked { get; set; }

    public GamingGraphicsCard(string manufacturer, string model, decimal price, int powerConsumption,
                            int memoryGB, string memoryType, double coreClock, int cudaCores,
                            bool hasRGB, int fansCount, bool overclocked)
        : base(manufacturer, model, price, powerConsumption, memoryGB, memoryType, coreClock, cudaCores)
    {
        HasRGB = hasRGB;
        FansCount = fansCount;
        Overclocked = overclocked;
    }

    public override Component MyClone()
    {
        return new GamingGraphicsCard(Manufacturer, Model, Price, PowerConsumption,
                                    MemoryGB, MemoryType, CoreClock, CudaCores,
                                    HasRGB, FansCount, Overclocked);
    }

    GamingGraphicsCard IMyCloneable<GamingGraphicsCard>.MyClone()
    {
        return new GamingGraphicsCard(Manufacturer, Model, Price, PowerConsumption,
                                    MemoryGB, MemoryType, CoreClock, CudaCores,
                                    HasRGB, FansCount, Overclocked);
    }

    public override string ToString()
    {
        return $"Gaming Graphics Card: {Manufacturer} {Model}, {MemoryGB}GB {MemoryType}, " +
               $"{CoreClock}MHz, {CudaCores} CUDA cores, Fans: {FansCount}, " +
               $"RGB: {(HasRGB ? "Yes" : "No")}, OC: {(Overclocked ? "Yes" : "No")}, " +
               $"Price: {Price:C}, {PowerConsumption}W";
    }
}

// Программа для демонстрации
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== Демонстрация паттерна Прототип (Компьютерное железо) ===\n");

        var genericComponent = new Component("Generic", "PC-Part", 50.00m, 10);

        var cpu = new Processor("Intel", "Core i7-13700K", 450.00m, 125,
                               5.4, 16, 24, "LGA1700");

        var gpu = new GraphicsCard("NVIDIA", "RTX 4070 Ti", 800.00m, 285,
                                 12, "GDDR6X", 2610, 7680);

        var motherboard = new Motherboard("ASUS", "ROG STRIX Z790-E", 400.00m, 30,
                                        "ATX", "Z790", "LGA1700", 4, 3);

        var gamingGpu = new GamingGraphicsCard("ASUS", "ROG STRIX RTX 4090", 1600.00m, 450,
                                             24, "GDDR6X", 2520, 16384,
                                             true, 3, true);

        var genericClone = genericComponent.MyClone();
        var cpuClone = ((IMyCloneable<Processor>)cpu).MyClone();
        var gpuClone = ((IMyCloneable<GraphicsCard>)gpu).MyClone();
        var motherboardClone = motherboard.MyClone() as Motherboard;
        var gamingGpuClone = ((IMyCloneable<GamingGraphicsCard>)gamingGpu).MyClone();

        var cpuCloneViaICloneable = cpu.Clone() as Processor;

        Console.WriteLine("=== ОРИГИНАЛЫ ===");
        Console.WriteLine(genericComponent);
        Console.WriteLine(cpu);
        Console.WriteLine(gpu);
        Console.WriteLine(motherboard);
        Console.WriteLine(gamingGpu);

        Console.WriteLine("\n=== КЛОНЫ ===");
        Console.WriteLine(genericClone);
        Console.WriteLine(cpuClone);
        Console.WriteLine(gpuClone);
        Console.WriteLine(motherboardClone);
        Console.WriteLine(gamingGpuClone);
        Console.WriteLine($"CPU Clone via ICloneable: {cpuCloneViaICloneable}");

        Console.WriteLine("\n=== ПРОВЕРКА НЕЗАВИСИМОСТИ КЛОНОВ ===");
        Console.WriteLine("Изменяем оригинал процессора: Manufacturer -> 'AMD', Model -> 'Ryzen 9 7950X3D'");

        cpu.Manufacturer = "AMD";
        cpu.Model = "Ryzen 9 7950X3D";
        cpu.Price = 700.00m;

        Console.WriteLine($"Оригинал после изменений: {cpu}");
        Console.WriteLine($"Клон (не изменился): {cpuClone}");

        Console.WriteLine("\n=== ТЕСТ С КОЛЛЕКЦИЕЙ КОМПЛЕКТУЮЩИХ ===");

        var computerBuild = new List<Component>
        {
            new Processor("AMD", "Ryzen 5 7600X", 300.00m, 105, 5.3, 6, 12, "AM5"),
            new GraphicsCard("NVIDIA", "RTX 4060 Ti", 400.00m, 160, 8, "GDDR6", 2535, 4352),
            new Motherboard("MSI", "B650 TOMAHAWK", 250.00m, 20, "ATX", "B650", "AM5", 4, 2),
            new GamingGraphicsCard("Gigabyte", "RTX 4080 AERO", 1200.00m, 320,
                                 16, "GDDR6X", 2505, 9728, true, 3, false)
        };

        var clonedBuild = new List<Component>();
        foreach (var component in computerBuild)
        {
            clonedBuild.Add((Component)component.Clone());
        }

        // Проверка типов клонов
        for (int i = 0; i < computerBuild.Count; i++)
        {
            Console.WriteLine($"Оригинал: {computerBuild[i].GetType().Name}, Клон: {clonedBuild[i].GetType().Name}");
            Console.WriteLine($"  Оригинал: {computerBuild[i]}");
            Console.WriteLine($"  Клон: {clonedBuild[i]}");
            Console.WriteLine($"  Разные объекты: {!ReferenceEquals(computerBuild[i], clonedBuild[i])}\n");
        }

        // Демонстрация использования обоих интерфейсов
        Console.WriteLine("=== ДЕМОНСТРАЦИЯ РАБОТЫ ОБОИХ ИНТЕРФЕЙСОВ ===");

        var testProcessor = new Processor("Intel", "i5-13600K", 320.00m, 125, 5.1, 14, 20, "LGA1700");

        Processor myCloneProcessor = ((IMyCloneable<Processor>)testProcessor).MyClone();

        Processor icloneableProcessor = (Processor)testProcessor.Clone();

        Console.WriteLine($"Оригинал: {testProcessor}");
        Console.WriteLine($"MyClone: {myCloneProcessor}");
        Console.WriteLine($"ICloneable: {icloneableProcessor}");
        Console.WriteLine($"Тип MyClone результата: {myCloneProcessor.GetType().Name}");
        Console.WriteLine($"Тип ICloneable результата: {icloneableProcessor.GetType().Name}");
    }
}