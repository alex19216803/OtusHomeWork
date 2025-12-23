using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Otus_HomeWork2ADONet.Configuration
{
    public class AppSettings
    {
        public string PostgresServer { get; set; } = "localhost";
        public int PostgresPort { get; set; } = 5432;
        public string PostgresUsername { get; set; } = "postgres";
        public string PostgresPassword { get; set; } = "";
        public string DatabaseName { get; set; } = "VirtualShopDB";

        public static AppSettings Load()
        {
            string SettingsFilePath = "Otus_HomeWork2ADONet.settings";
            try
            {
                if (File.Exists(SettingsFilePath))
                {
                    var json = File.ReadAllText(SettingsFilePath);
                    var settings = JsonSerializer.Deserialize<AppSettings>(json);

                    if (settings != null)
                    {
                        Console.WriteLine($"Настройки загружены из файла: {SettingsFilePath}");
                        return settings;
                    }
                }

                Console.WriteLine("Файл настроек не найден. Используются значения по умолчанию.");
                return new AppSettings();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки настроек: {ex.Message}");
                return new AppSettings();
            }
        }

    }
}
