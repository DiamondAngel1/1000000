using System;
using System.Text;
using System.Diagnostics;
using Bogus;
using System.Data.SqlClient;
using System.Data;

namespace _1000000Users{
    class Program{
        static void Main(){
            Console.InputEncoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.UTF8;
            var str = ConnectManager.GetConnectionString();
            var categoryManager = new CategoryManager(str);
            try{
                using (var con = new SqlConnection(str)){
                    con.Open();
                    Console.WriteLine("Підключення успішне");
                }
            }
            catch (Exception ex){
                Console.WriteLine($"Якась помилка: {ex.Message}");
            }
            int operation;
            Faker faker = new Faker("en");
            do{
                Console.WriteLine("Оберіть варіант: ");
                Console.WriteLine("0. Вихід");
                Console.WriteLine("1. Створити таблицю");
                Console.WriteLine("2. Додати 1000000 користувачів");
                Console.WriteLine("3. Підрахувати скільки місця займає таблиця");
                Console.WriteLine("4. Зробити пошук користувачів за іменем");
                Console.WriteLine("5. Зробити пошук користувачів за прізвищем");
                Console.WriteLine("6. Зробити пошук користувачів за номером телефону");
                Console.WriteLine("7. Видалити таблицю");
                operation = Convert.ToInt32(Console.ReadLine());
                switch (operation){
                    case 0:
                        Console.WriteLine("Вихід з програми :)");
                        break;
                    case 1:
                        using (var con = new SqlConnection(str)){
                            con.Open();
                            var command = con.CreateCommand();
                            command.CommandText = @"IF NOT EXISTS (SELECT * FROM sys.tables WHERE object_id = OBJECT_ID(N'Users')) EXEC dbo.sp_executesql @statement = N'CREATE TABLE Users (Id INT PRIMARY KEY IDENTITY(1,1), FirstName NVARCHAR(50) NOT NULL, LastName NVARCHAR(50) NOT NULL, Phone NVARCHAR(50));'";
                            try{
                                command.ExecuteNonQuery();
                                Console.WriteLine("Таблиця створена");
                            }
                            catch (Exception e){
                                Console.WriteLine($"Якась помилка: {e.Message}");
                            }
                        }
                        break;
                    case 2:
                        using (var con = new SqlConnection(str)){
                            con.Open();
                            Stopwatch stopWatch = new Stopwatch();
                            stopWatch.Start();
                            DataTable usersTable = new DataTable();
                            usersTable.Columns.Add("FirstName", typeof(string));
                            usersTable.Columns.Add("LastName", typeof(string));
                            usersTable.Columns.Add("Phone", typeof(string));
                            for (int i = 0; i < 1000000; i++){
                                usersTable.Rows.Add(faker.Name.FirstName(), faker.Name.LastName(), faker.Phone.PhoneNumberFormat());
                            }
                            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(con)){
                                bulkCopy.DestinationTableName = "Users";
                                bulkCopy.ColumnMappings.Add("FirstName", "FirstName");
                                bulkCopy.ColumnMappings.Add("LastName", "LastName");
                                bulkCopy.ColumnMappings.Add("Phone", "Phone");
                                try{
                                    bulkCopy.WriteToServer(usersTable);
                                }
                                catch (Exception e){
                                    Console.WriteLine($"Якась помилка: {e.Message}");
                                }
                            }
                            stopWatch.Stop();
                            TimeSpan ts = stopWatch.Elapsed;
                            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                                ts.Hours, ts.Minutes, ts.Seconds,
                                ts.Milliseconds / 10);
                            Console.WriteLine($"Додавання користувачів зайняло: {elapsedTime}");
                        }
                        break;
                    case 3:
                        using (var con = new SqlConnection(str)){
                            con.Open();
                            var command = con.CreateCommand();
                            command.CommandText = "EXEC sp_spaceused 'Users';";
                            try{
                                using (var reader = command.ExecuteReader()){
                                    if (reader.Read()){
                                        string tableName = reader["name"].ToString();
                                        string quantRows = reader["rows"].ToString();
                                        string dataSize = reader["data"].ToString();
                                        string indexSize = reader["index_size"].ToString();
                                        string unused = reader["unused"].ToString();
                                        double totalSize = Convert.ToDouble(reader["reserved"].ToString().Replace(" KB", ""));
                                        double totalSizeMb = totalSize/1024;
                                        Console.WriteLine($"\nІнформація про таблицю '{tableName}':");
                                        Console.WriteLine($"Кількість рядків: {quantRows}");
                                        Console.WriteLine($"Дані: {dataSize}");
                                        Console.WriteLine($"Індекси: {indexSize}");
                                        Console.WriteLine($"Не використовується: {unused}");
                                        Console.WriteLine($"Загальний розмір в Кб: {totalSize} КБ");
                                        Console.WriteLine($"Загальний розмір в Мб: {totalSizeMb} МБ\n");
                                    }
                                }
                            }
                            catch (Exception e){
                                Console.WriteLine($"Якась помилка: {e.Message}");
                            }
                        }
                        break;
                    case 4:
                        Console.WriteLine("Введіть ім'я для пошуку:");
                        string searchName = Console.ReadLine().Trim();
                        using (var con = new SqlConnection(str)){
                            con.Open();
                            var command = con.CreateCommand();
                            command.CommandText = @"SELECT * FROM Users WHERE FirstName = @searchName";
                            command.Parameters.AddWithValue("@searchName", searchName);
                            try{
                                using (var reader = command.ExecuteReader()){
                                    bool found = false;
                                    while (reader.Read()){
                                        found = true;
                                        string id = reader["id"].ToString();
                                        string name = reader["FirstName"].ToString();
                                        string lastName = reader["LastName"].ToString();
                                        string phone = reader["Phone"].ToString();
                                        Console.WriteLine($"Id: {id}, Ім'я: {name} {lastName} Телефон: {phone}");
                                    }
                                    if (!found){
                                        Console.WriteLine("Користувача з таким ім'ям не знайдено.");
                                    }
                                }
                            }
                            catch (Exception e){
                                Console.WriteLine($"Якась помилка: {e.Message}");
                            }
                        }
                        break;

                    case 5:
                        Console.WriteLine("Введіть прізвище для пошуку:");
                        string searchLastName = Console.ReadLine().Trim();
                        using (var con = new SqlConnection(str)){
                            con.Open();
                            var command = con.CreateCommand();
                            command.CommandText = @"SELECT * FROM Users WHERE LastName = @searchLastName";
                            command.Parameters.AddWithValue("@searchLastName", searchLastName);
                            try{
                                using (var reader = command.ExecuteReader()){
                                    bool found = false;
                                    while (reader.Read()){
                                        found = true;
                                        string id = reader["id"].ToString();
                                        string name = reader["FirstName"].ToString();
                                        string lastName = reader["LastName"].ToString();
                                        string phone = reader["Phone"].ToString();
                                        Console.WriteLine($"Id: {id}, Ім'я: {name} {lastName} Телефон: {phone}");
                                    }
                                    if (!found){
                                        Console.WriteLine("Користувача з таким прізвищем не знайдено.");
                                    }
                                }
                            }
                            catch (Exception e){
                                Console.WriteLine($"Якась помилка: {e.Message}");
                            }
                        }
                        break;
                    case 6:
                        Console.WriteLine("Введіть номер телефону для пошуку:");
                        string searchPhone = Console.ReadLine().Trim();
                        using (var con = new SqlConnection(str)){
                            con.Open();
                            var command = con.CreateCommand();
                            command.CommandText = @"SELECT * FROM Users WHERE Phone = @searchPhone";
                            command.Parameters.AddWithValue("@searchPhone", searchPhone);
                            try{
                                using (var reader = command.ExecuteReader()){
                                    bool found = false;
                                    while (reader.Read()){
                                        found = true;
                                        string id = reader["id"].ToString();
                                        string name = reader["FirstName"].ToString();
                                        string lastName = reader["LastName"].ToString();
                                        string phone = reader["Phone"].ToString();
                                        Console.WriteLine($"Id: {id}, Ім'я: {name} {lastName} Телефон: {phone}");
                                    }
                                    if (!found){
                                        Console.WriteLine("Користувача з таким номером телефону не знайдено.");
                                    }
                                }
                            }
                            catch (Exception e){
                                Console.WriteLine($"Якась помилка: {e.Message}");
                            }
                        }
                        break;

                    case 7:
                        using (var con = new SqlConnection(str)){
                            con.Open();
                            var command = con.CreateCommand();
                            command.CommandText = @"DROP TABLE Users";
                            try{
                                command.ExecuteNonQuery();
                                Console.WriteLine("Таблиця видалена");
                            }
                            catch (Exception e){
                                Console.WriteLine($"Якась помилка: {e.Message}");
                            }
                        }
                        break;
                    default:
                        Console.WriteLine("Невідомий варіант");
                        break;
                }
            } while (operation != 0);
        }
    }
}
