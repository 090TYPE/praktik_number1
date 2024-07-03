using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace prak1
{
    internal class Program
    {
        static void Main()
        {
            string connectionString = "Server=090LAPTOP;Database=1kom;Integrated Security=True;";
            Console.WriteLine("Выберите задачу:");
            Console.WriteLine("1: Вывести модули, к которым обращалось менее 10 разных пользователей помесячно.");
            Console.WriteLine("2: Вывести список обращений для заданного plugin_uid.");
            int taskChoice;
            while (!int.TryParse(Console.ReadLine(), out taskChoice) || (taskChoice != 1 && taskChoice != 2))
            {
                Console.WriteLine("Пожалуйста, введите 1 или 2 для выбора задачи.");
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    if (taskChoice == 1)
                    {
                        ExecuteTask1(connection);
                    }
                    else if (taskChoice == 2)
                    {
                        ExecuteTask2(connection);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Произошла ошибка при подключении к базе данных или выполнении запроса.");
                    Console.WriteLine(ex.Message);
                }
            }
            // Ожидание пользовательского ввода перед завершением
            Console.WriteLine("Нажмите Enter для завершения программы...");
            Console.ReadLine();
        }

        static void ExecuteTask1(SqlConnection connection)
        {
            string query = @"
            WITH MonthlyModuleUsage AS (
                SELECT
                    plugin_uid,
                    DATEPART(MONTH, date) AS Month,
                    COUNT(DISTINCT user_id) AS UserCount
                FROM user_plugins
                GROUP BY plugin_uid, DATEPART(MONTH, date)
            )
            SELECT
                plugin_uid,
                [1] AS Jan,
                [2] AS Feb,
                [3] AS Mar,
                [4] AS Apr,
                [5] AS May,
                [6] AS Jun,
                [7] AS Jul,
                [8] AS Aug,
                [9] AS Sep,
                [10] AS Oct,
                [11] AS Nov,
                [12] AS Dec
            FROM
                MonthlyModuleUsage
            PIVOT (
                MAX(UserCount) FOR Month IN ([1], [2], [3], [4], [5], [6], [7], [8], [9], [10], [11], [12])
            ) AS PivotTable
            WHERE
                [1] < 10 OR [2] < 10 OR [3] < 10 OR [4] < 10 OR [5] < 10 OR [6] < 10 OR
                [7] < 10 OR [8] < 10 OR [9] < 10 OR [10] < 10 OR [11] < 10 OR [12] < 10
            ORDER BY plugin_uid;
        ";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    Console.WriteLine("\nModules with less than 10 different users monthly:");
                    while (reader.Read())
                    {
                        Console.WriteLine($"{reader["plugin_uid"]} | Jan: {reader["Jan"]}, Feb: {reader["Feb"]}, Mar: {reader["Mar"]}, Apr: {reader["Apr"]}, May: {reader["May"]}, Jun: {reader["Jun"]}, Jul: {reader["Jul"]}, Aug: {reader["Aug"]}, Sep: {reader["Sep"]}, Oct: {reader["Oct"]}, Nov: {reader["Nov"]}, Dec: {reader["Dec"]}");
                    }
                }
            }
        }

        static void ExecuteTask2(SqlConnection connection)
        {
            Console.Write("Введите plugin_uid для получения списка обращений: ");
            string pluginUid = Console.ReadLine();

            string query = @"
                    WITH Directories AS (
    SELECT id, title
    FROM Myotdels
    WHERE teg LIKE '%direction%'
),
RecursiveDepartments AS (
    SELECT id, parent_id, title, CAST(title AS VARCHAR(MAX)) AS path
    FROM Myotdels
    WHERE parent_id IS NULL
    UNION ALL
    SELECT d.id, d.parent_id, d.title, CAST(rd.path + ' -> ' + d.title AS VARCHAR(MAX))
    FROM Myotdels d
    INNER JOIN RecursiveDepartments rd ON d.parent_id = rd.id
)
SELECT
    d.title AS direction,
    u.fio AS user_name,
    up.date AS date_of_request,
    SUM(up.cnt) AS total_requests
FROM user_plugins up
INNER JOIN users u ON up.user_id = u.id
INNER JOIN RecursiveDepartments rd ON u.otdel_id = rd.id
INNER JOIN Directories d ON rd.path LIKE '%' + d.title + '%'
WHERE up.plugin_uid = @plugin_uid
GROUP BY d.title, u.fio, up.date
ORDER BY d.title, u.fio, up.date;
                ";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@plugin_uid", pluginUid);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    Console.WriteLine($"\nDetails for plugin_uid {pluginUid}:");
                    Console.WriteLine("----------------------------------------------");
                    Console.WriteLine("{0,-20} {1,-20} {2,-20} {3,-20}", "Direction", "User", "Date", "Total Requests");
                    Console.WriteLine("----------------------------------------------");

                    while (reader.Read())
                    {
                        string direction = reader["direction"].ToString();
                        string user = reader["user"].ToString();
                        string date = reader["date"].ToString();
                        int totalRequests = Convert.ToInt32(reader["total_requests"]);

                        Console.WriteLine("{0,-20} {1,-20} {2,-20} {3,-20}", direction, user, date, totalRequests);
                    }
                }
            }
        }
    }
}
