using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Threading.Tasks;

namespace evdokimovprc3
{
    internal class Program
    {
        private static string connectionString = "Server=sql.bsite.net\\MSSQL2016;Database=c418c418_;User Id=c418c418_;Password=13244231;TrustServerCertificate=true;";

        static async Task Main(string[] args)
        {
            SqlConnection connection = new SqlConnection(connectionString);
            try
            {
                await connection.OpenAsync();
                Console.WriteLine("Подключение открыто");
                Console.WriteLine($"\tБаза данных: {connection.Database}");

                while (true)
                {
                    Console.Clear();
                    Console.WriteLine("Работа с таблицей Users.");
                    Console.WriteLine("Выберите действие:");
                    Console.WriteLine("1) Посмотреть все записи");
                    Console.WriteLine("2) Добавить нового пользователя");
                    Console.WriteLine("3) Обновить существующего пользователя");
                    Console.WriteLine("4) Удалить существующего пользователя");
                    Console.WriteLine("5) Авторизоваться в системе");
                    Console.WriteLine("6) Выйти");

                    string choice = Console.ReadLine();

                    switch (choice)
                    {
                        case "1":
                            await ViewAllUsers(connection);
                            break;
                        case "2":
                            await AddNewUser(connection);
                            break;
                        case "3":
                            await UpdateUser(connection);
                            break;
                        case "4":
                            await DeleteUser(connection);
                            break;
                        case "5":
                            await AuthenticateUser(connection);
                            break;
                        case "6":
                            return;
                        default:
                            Console.WriteLine("Неверный выбор. Попробуйте снова.");
                            break;
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    await connection.CloseAsync();
                    Console.WriteLine("Подключение закрыто.");
                }
            }
            Console.WriteLine("Программа завершила работу.");
            Console.Read();
        }

        private static async Task ViewAllUsers(SqlConnection connection)
        {
            Console.Clear();
            SqlCommand command = new SqlCommand("SELECT * FROM Users", connection);
            SqlDataReader reader = await command.ExecuteReaderAsync();

            Console.WriteLine("Список пользователей:");
            Console.WriteLine("ID\tFirstname\tUsername\tPassword\tAge");

            while (await reader.ReadAsync())
            {
                Console.WriteLine($"{reader["UserID"]}\t{reader["Name"]}\t{reader["Email"]}\t********\t{reader["Role"]}");
            }

            reader.Close();
            Console.WriteLine("\nНажмите на любую клавишу чтобы вернуться.");
            Console.ReadKey();
        }

        private static async Task AddNewUser(SqlConnection connection)
        {
            Console.Clear();
            Console.WriteLine("Добавление пользователя:");
            Console.WriteLine("Введите следующие данные через запятую \",\"");
            Console.WriteLine("Firstname,Username,Password,Age");
            Console.WriteLine("Пример: Иван,Vanya,12345,50");

            string input;
            while (true)
            {
                input = Console.ReadLine();
                string[] data = input.Split(',');

                if (data.Length == 4 && !string.IsNullOrWhiteSpace(data[0]) && !string.IsNullOrWhiteSpace(data[1]) && !string.IsNullOrWhiteSpace(data[2]) && int.TryParse(data[3], out _))
                {
                    break;
                }
                else
                {
                    Console.WriteLine("Неверный формат, повторите еще раз.");
                }
            }

            string[] userData = input.Split(',');
            string query = "INSERT INTO Users (Name, Email, PasswordHash, Role) VALUES (@Name, @Email, @PasswordHash, @Role); SELECT SCOPE_IDENTITY();";
            SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Name", userData[0]);
            command.Parameters.AddWithValue("@Email", userData[1]);
            command.Parameters.AddWithValue("@PasswordHash", userData[2]);
            command.Parameters.AddWithValue("@Role", userData[3]);

            int newUserId = Convert.ToInt32(await command.ExecuteScalarAsync());
            Console.WriteLine($"Добавление успешно! Его Id - {newUserId}.");
            Console.WriteLine("\nНажмите на любую клавишу чтобы вернуться.");
            Console.ReadKey();
        }

        private static async Task UpdateUser(SqlConnection connection)
        {
            Console.Clear();
            Console.WriteLine("Обновление пользователя:");
            Console.WriteLine("Введите ID пользователя, которого хотите обновить:");
            int userId = int.Parse(Console.ReadLine());

            Console.WriteLine("Введите новые данные через запятую \",\"");
            Console.WriteLine("Firstname,Username,Password,Age");
            Console.WriteLine("Пример: Иван,Vanya,12345,50");

            string input;
            while (true)
            {
                input = Console.ReadLine();
                string[] data = input.Split(',');

                if (data.Length == 4 && !string.IsNullOrWhiteSpace(data[0]) && !string.IsNullOrWhiteSpace(data[1]) && !string.IsNullOrWhiteSpace(data[2]) && int.TryParse(data[3], out _))
                {
                    break;
                }
                else
                {
                    Console.WriteLine("Неверный формат, повторите еще раз.");
                }
            }

            string[] userData = input.Split(',');
            string query = "UPDATE Users SET Name = @Name, Email = @Email, PasswordHash = @PasswordHash, Role = @Role WHERE UserID = @UserID";
            SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Name", userData[0]);
            command.Parameters.AddWithValue("@Email", userData[1]);
            command.Parameters.AddWithValue("@PasswordHash", userData[2]);
            command.Parameters.AddWithValue("@Role", userData[3]);
            command.Parameters.AddWithValue("@UserID", userId);

            int rowsAffected = await command.ExecuteNonQueryAsync();
            if (rowsAffected > 0)
            {
                Console.WriteLine("Обновление успешно!");
            }
            else
            {
                Console.WriteLine("Пользователь с таким ID не найден.");
            }

            Console.WriteLine("\nНажмите на любую клавишу чтобы вернуться.");
            Console.ReadKey();
        }

        private static async Task DeleteUser(SqlConnection connection)
        {
            Console.Clear();
            Console.WriteLine("Удаление пользователя:");
            Console.WriteLine("Введите ID пользователя, которого хотите удалить:");
            int userId = int.Parse(Console.ReadLine());

            string query = "DELETE FROM Users WHERE UserID = @UserID";
            SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@UserID", userId);

            int rowsAffected = await command.ExecuteNonQueryAsync();
            if (rowsAffected > 0)
            {
                Console.WriteLine("Удаление успешно!");
            }
            else
            {
                Console.WriteLine("Пользователь с таким ID не найден.");
            }

            Console.WriteLine("\nНажмите на любую клавишу чтобы вернуться.");
            Console.ReadKey();
        }

        private static async Task AuthenticateUser(SqlConnection connection)
        {
            Console.Clear();
            Console.WriteLine("Авторизация пользователя:");
            Console.WriteLine("Введите Email и Password через запятую \",\"");
            Console.WriteLine("Пример: user@example.com,password");

            string input;
            while (true)
            {
                input = Console.ReadLine();
                string[] data = input.Split(',');

                if (data.Length == 2 && !string.IsNullOrWhiteSpace(data[0]) && !string.IsNullOrWhiteSpace(data[1]))
                {
                    break;
                }
                else
                {
                    Console.WriteLine("Неверный формат, повторите еще раз.");
                }
            }

            string[] authData = input.Split(',');
            string query = "SELECT * FROM Users WHERE Email = @Email AND PasswordHash = @PasswordHash";
            SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Email", authData[0]);
            command.Parameters.AddWithValue("@PasswordHash", authData[1]);

            SqlDataReader reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                Console.WriteLine("Авторизация успешна!");
            }
            else
            {
                Console.WriteLine("Неверный Email или Password.");
            }

            reader.Close();
            Console.WriteLine("\nНажмите на любую клавишу чтобы вернуться.");
            Console.ReadKey();
        }
    }
}