using System;
using System.Data;
using Npgsql;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PostgresManager
{
    public class PostgresDatabaseManager
    {
        public bool PrimaryKeyExists(string tableName)
        {
            string sql = @"
        SELECT EXISTS (
            SELECT 1 
            FROM pg_constraint con
            JOIN pg_class rel ON rel.oid = con.conrelid
            JOIN pg_namespace nsp ON nsp.oid = rel.relnamespace
            WHERE nsp.nspname = 'public'
              AND rel.relname = @tableName
              AND con.contype = 'p'
        )";

            using (var connection = new NpgsqlConnection(_connectionString))
            using (var cmd = new NpgsqlCommand(sql, connection))
            {
                cmd.Parameters.AddWithValue("@tableName", tableName);
                connection.Open();
                return (bool)cmd.ExecuteScalar();
            }
        }
        private string _connectionString;
        private string _databaseName;

        /// <summary>
        /// Конструктор для подключения к серверу PostgreSQL
        /// </summary>
        /// <param name="server">Адрес сервера</param>
        /// <param name="username">Имя пользователя</param>
        /// <param name="password">Пароль</param>
        /// <param name="databaseName">Имя базы данных (необязательно)</param>
        /// <param name="port">Порт (по умолчанию 5432)</param>
        public PostgresDatabaseManager(string server, string username, string password, string databaseName = "postgres", int port = 5432)
        {
            _databaseName = databaseName;

            var builder = new NpgsqlConnectionStringBuilder
            {
                Host = server,
                Port = port,
                Username = username,
                Password = password,
                Database = databaseName, 
                Pooling = false, 
                Timeout = 60
            };

            _connectionString = builder.ConnectionString;
        }

        public int Update(string tableName, Dictionary<string, object> updates, Dictionary<string, object> whereConditions = null)
        {
            if (string.IsNullOrEmpty(tableName))
                throw new ArgumentException("Имя таблицы не может быть пустым");

            if (updates == null || updates.Count == 0)
                throw new ArgumentException("Не указаны поля для обновления");

            using (var connection = new NpgsqlConnection(GetDatabaseConnectionString()))
            {
                try
                {
                    connection.Open();

                    var setClauses = new List<string>();
                    var parameters = new List<NpgsqlParameter>();
                    int paramIndex = 0;

                    foreach (var update in updates)
                    {
                        var paramName = $"@p{paramIndex++}";
                        setClauses.Add($"\"{update.Key}\" = {paramName}");
                        parameters.Add(new NpgsqlParameter(paramName, update.Value ?? DBNull.Value));
                    }

                    var whereClauses = new List<string>();
                    if (whereConditions != null && whereConditions.Count > 0)
                    {
                        foreach (var condition in whereConditions)
                        {
                            var paramName = $"@w{paramIndex++}";
                            whereClauses.Add($"\"{condition.Key}\" = {paramName}");
                            parameters.Add(new NpgsqlParameter(paramName, condition.Value ?? DBNull.Value));
                        }
                    }

                    var query = $"UPDATE \"{tableName}\" SET {string.Join(", ", setClauses)}";
                    if (whereClauses.Count > 0)
                    {
                        query += $" WHERE {string.Join(" AND ", whereClauses)}";
                    }

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddRange(parameters.ToArray());
                        var result = command.ExecuteNonQuery();

                        Console.WriteLine($"Обновлено {result} строк в таблице '{tableName}'");
                        return result;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при обновлении данных: {ex.Message}");
                    throw;
                }
            }
        }

        /// <summary>
        /// Создание базы данных
        /// </summary>
        /// <param name="databaseName">Имя создаваемой базы данных</param>
        public void CreateDatabase(string databaseName)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();
                    var checkQuery = $"SELECT 1 FROM pg_database WHERE datname = '{databaseName}'";
                    using (var checkCmd = new NpgsqlCommand(checkQuery, connection))
                    {
                        var exists = checkCmd.ExecuteScalar();

                        if (exists == null)
                        {
                            var query = $"CREATE DATABASE \"{databaseName}\" ENCODING = 'UTF8' LC_COLLATE = 'Russian_Russia.1251' LC_CTYPE = 'Russian_Russia.1251' TEMPLATE = template0;";
                            using (var command = new NpgsqlCommand(query, connection))
                            {
                                command.ExecuteNonQuery();
                                Console.WriteLine($"База данных '{databaseName}' успешно создана.");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"База данных '{databaseName}' уже существует.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при создании базы данных: {ex.Message}");
                    throw new Exception("Ошибка при создании БД");
                }
            }
        }

        /// <summary>
        /// Создание таблицы в БД
        /// </summary>
        /// <param name="tableName">Имя таблицы</param>
        /// <param name="columns">Словарь с именами и типами колонок</param>
        public void CreateTable(string tableName, Dictionary<string, string> columns, string schema = "public")
        {
            using (var connection = new NpgsqlConnection(GetDatabaseConnectionString()))
            {
                try
                {
                    connection.Open();

                    var columnDefinitions = new List<string>();
                    foreach (var column in columns)
                    {
                        columnDefinitions.Add($"\"{column.Key}\" {column.Value}");
                    }

                    var query = $"CREATE TABLE IF NOT EXISTS \"{schema}\".\"{tableName}\" ({string.Join(", ", columnDefinitions)});";

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.ExecuteNonQuery();
                        Console.WriteLine($"Таблица '\"{schema}\".\"{tableName}\"' успешно создана.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при создании таблицы: {ex.Message}");
                    throw;
                }
            }
        }

        /// <summary>
        /// Добавление полей в таблицу
        /// </summary>
        /// <param name="tableName">Имя таблицы</param>
        /// <param name="columns">Словарь с именами, типами и дополнительными параметрами колонок</param>
        /// <param name="primaryKeyColumn">Колонка для первичного ключа (необязательно)</param>
        /// <param name="foreignKeys">Список внешних ключей (необязательно)</param>
        public void AddColumnsToTable(string tableName, Dictionary<string, ColumnDefinition> columns, string primaryKeyColumn, List<ForeignKey> foreignKeys)
        {
            using (var connection = new NpgsqlConnection(GetDatabaseConnectionString()))
            {
                try
                {
                    connection.Open();

                    foreach (var column in columns)
                    {
                        var query = $"ALTER TABLE \"{tableName}\" ADD COLUMN \"{column.Key}\" {column.Value.DataType}";

                        if (!string.IsNullOrEmpty(column.Value.DefaultValue))
                        {
                            query += $" DEFAULT {column.Value.DefaultValue}";
                        }

                        if (column.Value.IsNotNull)
                        {
                            query += " NOT NULL";
                        }

                        query += ";";

                        using (var command = new NpgsqlCommand(query, connection))
                        {
                            command.ExecuteNonQuery();
                            Console.WriteLine($"Колонка '{column.Key}' добавлена в таблицу '{tableName}'.");
                        }
                    }

                    if (!string.IsNullOrEmpty(primaryKeyColumn))
                    {
                        var pkQuery = $"ALTER TABLE \"{tableName}\" ADD CONSTRAINT pk_{tableName}_{primaryKeyColumn} PRIMARY KEY (\"{primaryKeyColumn}\");";
                        using (var command = new NpgsqlCommand(pkQuery, connection))
                        {
                            command.ExecuteNonQuery();
                            Console.WriteLine($"Первичный ключ для колонки '{primaryKeyColumn}' создан.");
                        }
                    }

                    if (foreignKeys != null && foreignKeys.Count > 0)
                    {
                        foreach (var fk in foreignKeys)
                        {
                            var fkQuery = $"ALTER TABLE \"{tableName}\" ADD CONSTRAINT fk_{tableName}_{fk.ColumnName} " +
                                         $"FOREIGN KEY (\"{fk.ColumnName}\") REFERENCES \"{fk.ReferencedTable}\" (\"{fk.ReferencedColumn}\")";

                            if (fk.OnDelete != ForeignKeyAction.None)
                            {
                                fkQuery += $" ON DELETE {GetForeignKeyAction(fk.OnDelete)}";
                            }

                            if (fk.OnUpdate != ForeignKeyAction.None)
                            {
                                fkQuery += $" ON UPDATE {GetForeignKeyAction(fk.OnUpdate)}";
                            }

                            fkQuery += ";";

                            using (var command = new NpgsqlCommand(fkQuery, connection))
                            {
                                command.ExecuteNonQuery();
                                Console.WriteLine($"Внешний ключ для колонки '{fk.ColumnName}' создан.");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при добавлении колонок: {ex.Message}");
                    throw;
                }
            }
        }

        /// <summary>
        /// Чтение данных в DataTable
        /// </summary>
        /// <param name="query">SQL запрос для выборки данных</param>
        /// <returns>DataTable с результатами запроса</returns>
        public DataTable ReadData(string query)
        {
            var dataTable = new DataTable();

            using (var connection = new NpgsqlConnection(GetDatabaseConnectionString()))
            {
                try
                {
                    connection.Open();

                    using (var command = new NpgsqlCommand(query, connection))
                    using (var adapter = new NpgsqlDataAdapter(command))
                    {
                        adapter.Fill(dataTable);
                    }

                    Console.WriteLine($"Получено {dataTable.Rows.Count} записей.");
                    return dataTable;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при чтении данных: {ex.Message}");
                    throw;
                }
            }
        }

        /// <summary>
        /// Выполнение произвольного запроса без получения данных
        /// </summary>
        /// <param name="query">SQL запрос</param>
        /// <returns>Количество затронутых строк</returns>
        public int ExecuteNonQuery(string query)
        {
            using (var connection = new NpgsqlConnection(GetDatabaseConnectionString()))
            {
                try
                {
                    connection.Open();

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        var result = command.ExecuteNonQuery();
                        Console.WriteLine($"Выполнен запрос. Затронуто строк: {result}");
                        return result;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при выполнении запроса: {ex.Message}");
                    throw;
                }
            }
        }
        public bool ForeignKeyExists(string constraintName)
        {
            string sql = @"
        SELECT EXISTS (
            SELECT 1 
            FROM pg_constraint 
            WHERE conname = @constraintName 
              AND contype = 'f'
        )";

            using (var connection = new NpgsqlConnection(_connectionString))
            using (var cmd = new NpgsqlCommand(sql, connection))
            {
                cmd.Parameters.AddWithValue("@constraintName", constraintName);
                connection.Open();
                return (bool)cmd.ExecuteScalar();
            }
        }
        /// <summary>
        /// Удаление таблицы
        /// </summary>
        /// <param name="tableName">Имя таблицы</param>
        public void DropTable(string tableName)
        {
            ExecuteNonQuery($"DROP TABLE IF EXISTS \"{tableName}\" CASCADE;");
            Console.WriteLine($"Таблица '{tableName}' удалена (если существовала).");
        }

        /// <summary>
        /// Создание представления
        /// </summary>
        /// <param name="viewName">Имя представления</param>
        /// <param name="selectQuery">SELECT запрос для представления</param>
        public void CreateView(string viewName, string selectQuery)
        {
            var query = $"CREATE OR REPLACE VIEW \"{viewName}\" AS {selectQuery};";
            ExecuteNonQuery(query);
            Console.WriteLine($"Представление '{viewName}' создано/обновлено.");
        }

        /// <summary>
        /// Удаление представления
        /// </summary>
        /// <param name="viewName">Имя представления</param>
        public void DropView(string viewName)
        {
            ExecuteNonQuery($"DROP VIEW IF EXISTS \"{viewName}\" CASCADE;");
            Console.WriteLine($"Представление '{viewName}' удалено (если существовало).");
        }

         /// <summary>
        /// Получение строки подключения с указанием базы данных
        /// </summary>
        private string GetDatabaseConnectionString()
        {
            if (string.IsNullOrEmpty(_databaseName))
            {
                return _connectionString;
            }

            var builder = new NpgsqlConnectionStringBuilder(_connectionString)
            {
                Database = _databaseName
            };
            return builder.ConnectionString;
        }

        /// <summary>
        /// Преобразование действия внешнего ключа в строку SQL
        /// </summary>
        public string GetForeignKeyAction(ForeignKeyAction action)
        {
            return action switch
            {
                ForeignKeyAction.Cascade => "CASCADE",
                ForeignKeyAction.SetNull => "SET NULL",
                ForeignKeyAction.SetDefault => "SET DEFAULT",
                ForeignKeyAction.Restrict => "RESTRICT",
                _ => "NO ACTION"
            };
        }
        public int Insert(string tableName, Dictionary<string, object> data)
        {
            if (data == null || data.Count == 0)
                throw new ArgumentException("Данные для вставки не могут быть пустыми");
 
            var columns = string.Join(", ", data.Keys.Select(k => $"\"{k}\""));
            var parameters = string.Join(", ", data.Keys.Select(k => $"@{k}"));

            string sql = $@"INSERT INTO ""public"".""{tableName}"" ({columns}) VALUES ({parameters}) 
                    RETURNING *;";

            using (var connection = new NpgsqlConnection(_connectionString))
            using (var cmd = new NpgsqlCommand(sql, connection))
            {
                // Добавляем параметры
                foreach (var item in data)
                {
                    cmd.Parameters.AddWithValue($"@{item.Key}", item.Value ?? DBNull.Value);
                }

                connection.Open();
                try
                {
                    int result = (int)cmd.ExecuteScalar();
                    return result != null ? result : 0;
                }
                catch (Exception ex) 
                {
                    Console.WriteLine(ex);
                    return 0;
                }
            }
        }
        /// <summary>
        /// Вывод DataTable с цветовым форматированием
        /// </summary>
        public static void PrintDataTableWithColors(DataTable dataTable, Dictionary<string, ConsoleColor> columnColors = null)
        {
            if (dataTable == null || dataTable.Rows.Count == 0)
            {
                Console.WriteLine("Нет данных для отображения");
                return;
            }

            // Определяем ширину столбцов
            int[] columnWidths = new int[dataTable.Columns.Count];
            for (int i = 0; i < dataTable.Columns.Count; i++)
            {
                columnWidths[i] = Math.Max(dataTable.Columns[i].ColumnName.Length, 10);
            }

            for (int i = 0; i < Math.Min(dataTable.Rows.Count, 100); i++) // Ограничиваем для производительности
            {
                DataRow row = dataTable.Rows[i];
                for (int j = 0; j < dataTable.Columns.Count; j++)
                {
                    string cellValue = GetCellDisplayValue(row[j]);
                    columnWidths[j] = Math.Max(columnWidths[j], Math.Min(cellValue.Length, 30));
                }
            }

            // Выводим заголовки с цветом
            Console.WriteLine("\n" + new string('=', 80));
            Console.Write("|");

            for (int i = 0; i < dataTable.Columns.Count; i++)
            {
                ConsoleColor originalColor = Console.ForegroundColor;

                if (columnColors != null && columnColors.ContainsKey(dataTable.Columns[i].ColumnName))
                {
                    Console.ForegroundColor = columnColors[dataTable.Columns[i].ColumnName];
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                }

                Console.Write(" " + dataTable.Columns[i].ColumnName.PadRight(columnWidths[i]) + " ");
                Console.ForegroundColor = originalColor;
                Console.Write("|");
            }
            Console.WriteLine();
            Console.WriteLine(new string('=', 80));

            // Выводим данные
            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                DataRow row = dataTable.Rows[i];
                Console.Write("|");

                for (int j = 0; j < dataTable.Columns.Count; j++)
                {
                    string cellValue = GetCellDisplayValue(row[j]);

                    // Усекаем слишком длинные значения
                    if (cellValue.Length > columnWidths[j])
                    {
                        cellValue = cellValue.Substring(0, columnWidths[j] - 3) + "...";
                    }

                    // Применяем цветовое форматирование в зависимости от значения
                    ConsoleColor cellColor = GetCellColor(dataTable.Columns[j].ColumnName, row[j]);
                    ConsoleColor originalColor = Console.ForegroundColor;
                    Console.ForegroundColor = cellColor;

                    Console.Write(" " + cellValue.PadRight(columnWidths[j]) + " ");
                    Console.ForegroundColor = originalColor;
                    Console.Write("|");
                }
                Console.WriteLine();

                // Разделитель каждые 5 строк
                if ((i + 1) % 5 == 0 && i < dataTable.Rows.Count - 1)
                {
                    Console.WriteLine(new string('-', 80));
                }
            }

            Console.WriteLine(new string('=', 80));
            Console.WriteLine($"Всего строк: {dataTable.Rows.Count}");
        }
        /// <summary>
        /// Вспомогательный метод для форматирования значения ячейки
        /// </summary>
        private static string GetCellDisplayValue(object value)
        {
            if (value == null || value == DBNull.Value)
                return "NULL";

            if (value is DateTime dateTime)
                return dateTime.ToString("dd.MM.yyyy HH:mm");

            if (value is decimal decimalValue)
                return decimalValue.ToString("F2");

            if (value is double doubleValue)
                return doubleValue.ToString("F2");

            if (value is float floatValue)
                return floatValue.ToString("F2");

            return value.ToString();
        }

        /// <summary>
        /// Вывод разделительной линии таблицы
        /// </summary>
        private static void PrintTableLine(int[] columnWidths)
        {
            Console.Write("+");
            foreach (int width in columnWidths)
            {
                Console.Write(new string('-', width + 2) + "+");
            }
            Console.WriteLine();
        }
        /// <summary>
        /// Определение цвета ячейки на основе значения
        /// </summary>
        private static ConsoleColor GetCellColor(string columnName, object value)
        {
            if (value == null || value == DBNull.Value)
                return ConsoleColor.DarkGray;

            string stringValue = value.ToString().ToLower();

            // Примеры цветового форматирования
            switch (columnName.ToLower())
            {
                case string s when s.Contains("status"):
                    if (stringValue.Contains("error") || stringValue.Contains("failed") || stringValue.Contains("cancelled"))
                        return ConsoleColor.Red;
                    if (stringValue.Contains("success") || stringValue.Contains("completed") || stringValue.Contains("delivered"))
                        return ConsoleColor.Green;
                    if (stringValue.Contains("pending") || stringValue.Contains("processing"))
                        return ConsoleColor.Yellow;
                    break;

                case string s when s.Contains("amount") || s.Contains("price") || s.Contains("total"):
                    if (decimal.TryParse(stringValue, out decimal numericValue))
                    {
                        if (numericValue > 10000)
                            return ConsoleColor.Green;
                        if (numericValue < 0)
                            return ConsoleColor.Red;
                    }
                    break;

                case string s when s.Contains("quantity"):
                    if (int.TryParse(stringValue, out int quantity))
                    {
                        if (quantity == 0)
                            return ConsoleColor.Yellow;
                        if (quantity < 0)
                            return ConsoleColor.Red;
                    }
                    break;
            }

            return ConsoleColor.White;
        }
    }

    /// <summary>
    /// Класс для определения колонки
    /// </summary>
    public class ColumnDefinition
    {
        public string DataType { get; set; }
        public string DefaultValue { get; set; }
        public bool IsNotNull { get; set; }

        public ColumnDefinition(string dataType, string defaultValue, bool isNotNull = false)
        {
            DataType = dataType;
            DefaultValue = defaultValue;
            IsNotNull = isNotNull;
        }
    }

    /// <summary>
    /// Класс для определения внешнего ключа
    /// </summary>
    public class ForeignKey
    {
        public string ColumnName { get; set; }
        public string ReferencedTable { get; set; }
        public string ReferencedColumn { get; set; }
        public ForeignKeyAction OnDelete { get; set; }
        public ForeignKeyAction OnUpdate { get; set; }

        public ForeignKey(string columnName, string referencedTable, string referencedColumn,
                         ForeignKeyAction onDelete = ForeignKeyAction.None,
                         ForeignKeyAction onUpdate = ForeignKeyAction.None)
        {
            ColumnName = columnName;
            ReferencedTable = referencedTable;
            ReferencedColumn = referencedColumn;
            OnDelete = onDelete;
            OnUpdate = onUpdate;
        }
    }

    /// <summary>
    /// Действия для внешних ключей
    /// </summary>
    public enum ForeignKeyAction
    {
        None,
        Cascade,
        SetNull,
        SetDefault,
        Restrict,
        NoAction
    }

}