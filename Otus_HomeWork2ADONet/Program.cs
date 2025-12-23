using Otus_HomeWork2ADONet.Configuration;
using PostgresManager;
using System;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;



namespace Otus_HomeWork2ADONet
{
    internal class Program
    {

        static void Main(string[] args)
        {
            Console.WriteLine("Home work #2. ADO.Net Netunaev A.");

            AppSettings appSettings = AppSettings.Load();

            var manager = new PostgresDatabaseManager(appSettings.PostgresServer,appSettings.PostgresUsername, appSettings.PostgresPassword);

            manager.CreateDatabase(appSettings.DatabaseName);

            manager = new PostgresDatabaseManager(appSettings.PostgresServer, appSettings.PostgresUsername, appSettings.PostgresPassword, appSettings.DatabaseName);

            Dictionary<string,string> columns = new Dictionary<string,string>
            {
                { "ProductID", "SERIAL" },
                { "ProductName", "VARCHAR(200) NOT NULL" },
                { "Description", "TEXT" },
                { "Price", "DECIMAL(10,2) NOT NULL CHECK (\"Price\" >= 0)" },
                { "QuantityInStock", "INTEGER NOT NULL DEFAULT 0 CHECK (\"QuantityInStock\" >= 0)" },
                { "CreatedAt", "TIMESTAMP DEFAULT CURRENT_TIMESTAMP" },
                { "UpdatedAt", "TIMESTAMP DEFAULT CURRENT_TIMESTAMP" }
            };

            manager.CreateTable("Products", columns);
            manager.ExecuteNonQuery("CREATE INDEX IF NOT EXISTS idx_products_name ON \"Products\" (\"ProductName\");");
            manager.ExecuteNonQuery("CREATE INDEX IF NOT EXISTS idx_products_price ON \"Products\" (\"Price\");");

            if (!manager.PrimaryKeyExists("Products"))
                manager.ExecuteNonQuery("ALTER TABLE \"public\".\"Products\" ADD CONSTRAINT pk_products PRIMARY KEY (\"ProductID\");");

            Console.WriteLine("Таблица 'Products' создана успешно");

            columns = new Dictionary<string, string>
            {
                { "UserID", "SERIAL" },
                { "UserName", "VARCHAR(100) NOT NULL" },
                { "Email", "VARCHAR(255) NOT NULL UNIQUE" },
                { "RegistrationDate", "TIMESTAMP DEFAULT CURRENT_TIMESTAMP" },
                { "IsActive", "BOOLEAN DEFAULT true" },
            };

            manager.CreateTable("Users", columns);
            manager.ExecuteNonQuery("CREATE INDEX IF NOT EXISTS idx_users_email ON \"Users\" (\"Email\");");
            manager.ExecuteNonQuery("CREATE INDEX IF NOT EXISTS idx_users_registration ON \"Users\" (\"RegistrationDate\");");

            if (!manager.PrimaryKeyExists("Users"))
                manager.ExecuteNonQuery("ALTER TABLE \"public\".\"Users\" ADD CONSTRAINT pk_users PRIMARY KEY (\"UserID\");");

            Console.WriteLine("Таблица 'Users' создана успешно");

            columns = new Dictionary<string, string>
            {
                { "OrderID", "SERIAL" },
                { "UserID", "INTEGER NOT NULL" },
                { "OrderDate", "TIMESTAMP DEFAULT CURRENT_TIMESTAMP" },
                { "Status", "VARCHAR(50) DEFAULT 'Pending' CHECK (\"Status\" IN ('Pending', 'Processing', 'Shipped', 'Delivered', 'Cancelled'))" },
             };

            manager.CreateTable("Orders", columns);

            if (!manager.ForeignKeyExists("fk_orders_users"))
            {
                string fkUsers = @"
                    ALTER TABLE ""Orders""
                    ADD CONSTRAINT fk_orders_users
                    FOREIGN KEY (""UserID"")
                    REFERENCES ""Users"" (""UserID"")
                    ON DELETE CASCADE
                    ON UPDATE CASCADE;";
                manager.ExecuteNonQuery(fkUsers);
            }


            if (!manager.PrimaryKeyExists("Orders"))
                manager.ExecuteNonQuery("ALTER TABLE \"public\".\"Orders\" ADD CONSTRAINT pk_orders PRIMARY KEY (\"OrderID\");");

            Console.WriteLine("Таблица 'Orders' создана успешно");

            columns = new Dictionary<string, string>
            {
                { "OrderDetailID", "SERIAL" },
                { "OrderID", "INTEGER NOT NULL" },
                { "ProductID", "INTEGER NOT NULL" },
                { "Quantity", "INTEGER NOT NULL CHECK (\"Quantity\" > 0)" },
                { "TotalCost", "DECIMAL(10,2)" },
            };

            manager.CreateTable("OrderDetails", columns);

            if (!manager.PrimaryKeyExists("OrderDetails"))
                manager.ExecuteNonQuery("ALTER TABLE \"OrderDetails\" ADD CONSTRAINT pk_orderdetails PRIMARY KEY (\"OrderDetailID\");");

            if (!manager.ForeignKeyExists("fk_orderdetails_orders"))
            {
                string fkOrders = @"
                ALTER TABLE ""OrderDetails""
                ADD CONSTRAINT fk_orderdetails_orders
                FOREIGN KEY (""OrderID"")
                REFERENCES ""Orders"" (""OrderID"")
                ON DELETE CASCADE
                ON UPDATE CASCADE;";

                manager.ExecuteNonQuery(fkOrders);
            }

            if (!manager.ForeignKeyExists("fk_orderdetails_products"))
            {
                string fkOrders = @"
                ALTER TABLE ""OrderDetails""
                ADD CONSTRAINT fk_orderdetails_products
                FOREIGN KEY (""ProductID"")
                REFERENCES ""Products"" (""ProductID"")
                ON DELETE CASCADE
                ON UPDATE CASCADE;";

                manager.ExecuteNonQuery(fkOrders);
            }

            manager.ExecuteNonQuery("CREATE INDEX IF NOT EXISTS idx_orderdetails_order ON \"OrderDetails\" (\"OrderID\");");
            manager.ExecuteNonQuery("CREATE INDEX IF NOT EXISTS idx_orderdetails_product ON \"OrderDetails\" (\"ProductID\");");

            Console.WriteLine("Таблица 'OrderDetails' создана успешно");

            manager.ExecuteNonQuery("DELETE FROM \"OrderDetails\"");
            manager.ExecuteNonQuery("DELETE FROM \"Orders\"");
            manager.ExecuteNonQuery("DELETE FROM \"Users\"");
            manager.ExecuteNonQuery("DELETE FROM \"Products\"");

            Console.WriteLine("Данные удалены в учебныз целях");

            List<int> products = new List<int>();
            List<int> users = new List<int>();
            List<int> orders = new List<int>();

            int id;
            
            var data = new Dictionary<string, object>
                {
                    { "ProductName", "SSD Kingston 120Gb" },
                    { "Description", "Накопитель SSD SATA3 6.0Gb/s" },
                    { "Price", 2030.00 },
                    { "QuantityInStock", 11 }
                };

            id = manager.Insert("Products", data);
            products.Add(id);

            Console.WriteLine($"Добавлена запись с ProductsID: {id}");

            data = new Dictionary<string, object>
                {
                    { "ProductName", "PC3-12800 DIMM 240-pin 1.35В 1x8" },
                    { "Description", "Оперативная память DDR3 8GB" },
                    { "Price", 1620.00 },
                    { "QuantityInStock", 71 }
                };

            id = manager.Insert("Products", data);
            products.Add(id);

            Console.WriteLine($"Добавлена запись с ProductsID: {id}");

            data = new Dictionary<string, object>
                {
                    { "ProductName", "Samsung DDR3 2x4gb 1333 mhz 1.5V" },
                    { "Description", "Оперативная память DDR3 2x4GB" },
                    { "Price", 3720.00 },
                    { "QuantityInStock", 2 }
                };

            id = manager.Insert("Products", data);
            products.Add(id);

            Console.WriteLine($"Добавлена запись с ProductsID: {id}");

            data = new Dictionary<string, object>
                {
                    { "ProductName", "ASUS PRIME B660-PLUS D4, LGA 1700" },
                    { "Description", "Материнская плата ASUS PRIME  4xDDR4, ATX 90mb18x0-m1eay0" },
                    { "Price", 13660.00 },
                    { "QuantityInStock", 32 }
                };

            id = manager.Insert("Products", data);
            products.Add(id);

            Console.WriteLine($"Добавлена запись с ProductsID: {id}");

            data = new Dictionary<string, object>
                {
                    { "ProductName", "HDD Seagate 2Tb" },
                    { "Description", "Накопитель HDD SATA3 6.0Gb/s 7200Rpm" },
                    { "Price", 12000.00 },
                    { "QuantityInStock", 50 }
                };

            id = manager.Insert("Products", data);
            products.Add(id);

            Console.WriteLine($"Добавлена запись с ProductsID: {id}");

            data = new Dictionary<string, object>
                {
                    { "ProductName", "HDD Seagate 4Tb" },
                    { "Description", "Накопитель HDD SATA3 6.0Gb/s 7200Rpm" },
                    { "Price", 19000.00 },
                    { "QuantityInStock", 10 }
                };

            id = manager.Insert("Products", data);
            products.Add(id);

            Console.WriteLine($"Добавлена запись с ProductsID: {id}");

            var updates = new Dictionary<string, object>
            {
                { "Price", 1960.00 },
            };

            var where = new Dictionary<string, object>
            {
                { "ProductID", products[0] }
            };

            var updResult = manager.Update("Products", updates, where);

            Console.WriteLine($"Результат обновления записи с ID: {products[0]} : {updResult}");

            data = new Dictionary<string, object>
                {
                    { "UserName", "Karl Muller" },
                    { "Email", "karl@post.de" },
                    { "RegistrationDate", DateTime.Now }
                };

            id = manager.Insert("Users", data);
            users.Add(id);

            Console.WriteLine($"Добавлена запись с UsersID: {id}");

            data = new Dictionary<string, object>
                {
                    { "UserName", "German Gess" },
                    { "Email", "german@dw.de" },
                    { "RegistrationDate", DateTime.Now }
                };

            id = manager.Insert("Users", data);
            users.Add(id);

            Console.WriteLine($"Добавлена запись с UsersID: {id}");

            data = new Dictionary<string, object>
                {
                    { "UserID", users[0] },
                    { "OrderDate", DateTime.Now },
                    { "Status", "Processing" }
                };

            id = manager.Insert("Orders", data);
            orders.Add(id);

            Console.WriteLine($"Добавлена запись OrdersID: {id}");

            data = new Dictionary<string, object>
                {
                    { "UserID", users[0] },
                    { "OrderDate", DateTime.Now },
                    { "Status", "Processing" }
                };

            id = manager.Insert("Orders", data);
            orders.Add(id);

            Console.WriteLine($"Добавлена запись OrdersID: {id}");

            data = new Dictionary<string, object>
                {
                    { "UserID", users[1] },
                    { "OrderDate", DateTime.Now },
                    { "Status", "Shipped" }
                };

            id = manager.Insert("Orders", data);
            orders.Add(id);

            Console.WriteLine($"Добавлена запись OrdersID: {id}");

            data = new Dictionary<string, object>
                {
                    { "OrderID", orders[0] },
                    { "ProductID", products[0] },
                    { "Quantity", 2 },
                };

            id = manager.Insert("OrderDetails", data);

            Console.WriteLine($"Добавлена запись OrderDetailsID: {id}");

            data = new Dictionary<string, object>
                {
                    { "OrderID", orders[0] },
                    { "ProductID", products[1] },
                    { "Quantity", 1 },
                };

            id = manager.Insert("OrderDetails", data);

            Console.WriteLine($"Добавлена запись OrderDetailsID: {id}");

            data = new Dictionary<string, object>
                {
                    { "OrderID", orders[1] },
                    { "ProductID", products[0] },
                    { "Quantity", 4 },
                };

            id = manager.Insert("OrderDetails", data);

            Console.WriteLine($"Добавлена запись OrderDetailsID: {id}");

            data = new Dictionary<string, object>
                {
                    { "OrderID", orders[2] },
                    { "ProductID", products[0] },
                    { "Quantity", 1 },
                };

            id = manager.Insert("OrderDetails", data);

            Console.WriteLine($"Добавлена запись OrderDetailsID: {id}");

            data = new Dictionary<string, object>
                {
                    { "OrderID", orders[2] },
                    { "ProductID", products[1] },
                    { "Quantity", 16 },
                };

            id = manager.Insert("OrderDetails", data);

            Console.WriteLine($"Добавлена запись OrderDetailsID: {id}");

            manager.ExecuteNonQuery(
                @"UPDATE ""OrderDetails"" od
                  SET ""TotalCost"" = od.""Quantity"" * p.""Price""
                  FROM ""Products"" p 
                  WHERE p.""ProductID"" = od.""ProductID""");

            Console.WriteLine("Расчет сумм заказов произведен");

            Console.WriteLine("");

            Console.WriteLine($"Вывод заказов пользователя ID{users[0]}");

            string query = @$"
            SELECT 
                o.""OrderID"",
                o.""OrderDate"",
                o.""Status"",
                od.""TotalCost"",
                COUNT(od.""OrderDetailID"") as ""ItemsCount"",
                SUM(od.""Quantity"") as ""TotalItems""
            FROM ""Orders"" o
            INNER JOIN ""OrderDetails"" od ON o.""OrderID"" = od.""OrderID""
            WHERE o.""UserID"" = {users[0]}
            GROUP BY 
                o.""OrderID"", 
                o.""OrderDate"", 
                o.""Status"", 
                od.""TotalCost""";

            DataTable dt = manager.ReadData( query );

            PostgresDatabaseManager.PrintDataTableWithColors(dt);

            Console.WriteLine("");

            Console.WriteLine("Расчет общей стоимости заказов");

            query = @"
            SELECT o.""OrderID"", SUM(od.""TotalCost"") AS Summ
            FROM ""OrderDetails"" od
            INNER JOIN ""Orders"" o ON o.""OrderID"" = od.""OrderID""
            WHERE o.""Status"" NOT IN('Cancelled')
            GROUP BY o.""OrderID""";

            dt = manager.ReadData(query);

            PostgresDatabaseManager.PrintDataTableWithColors(dt);

            Console.WriteLine("");

            Console.WriteLine("Подсчет количества товаров на складе");

            query = @"
            SELECT p.""ProductID"", p.""ProductName"", (p.""QuantityInStock"" - COALESCE(SUM(od.""Quantity""), 0)) AS QuantityInStock
            FROM ""Products"" p
            LEFT JOIN ""OrderDetails"" od ON od.""ProductID"" = p.""ProductID""
            GROUP BY p.""ProductID"", p.""ProductName"", p.""QuantityInStock""";

            dt = manager.ReadData(query);

            PostgresDatabaseManager.PrintDataTableWithColors(dt);

            Console.WriteLine("");

            Console.WriteLine("Получение 5 самых дорогих товаров");

            query = @"
            SELECT p.""ProductID"", p.""ProductName"", p.""Price"" 
            FROM ""Products"" p
            ORDER BY p.""Price"" DESC";

            dt = manager.ReadData(query);

            PostgresDatabaseManager.PrintDataTableWithColors(dt);

            Console.WriteLine("");

            Console.WriteLine("Список товаров с низким запасом (менее 5 штук)");

            query = @"
            SELECT p.""ProductID"", p.""ProductName"", p.""QuantityInStock"" - SUM(od.""Quantity"")
            FROM ""Products"" p
            LEFT JOIN ""OrderDetails"" od ON od.""ProductID"" = p.""ProductID""
            GROUP BY p.""ProductID"", p.""ProductName"", p.""QuantityInStock""
            HAVING(p.""QuantityInStock"" - SUM(od.""Quantity"")) < 5";

            dt = manager.ReadData(query);

            PostgresDatabaseManager.PrintDataTableWithColors(dt);

            Console.WriteLine("");

        }
    }
  
 }
