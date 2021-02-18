using Microsoft.Data.SqlClient;
using System;

namespace _03.MinionNames
{
    class Program
    {
        static void Main(string[] args)
        {
            SqlConnection dbConnection = new SqlConnection(
                "Server=.\\SQLEXPRESS;DATABASE=MinionsDB;Integrated Security=true");

            dbConnection.Open();

            int id = int.Parse(Console.ReadLine());
            string vilainNameQuery = "SELECT Name FROM Villains WHERE Id = @Id";

            using var command = new SqlCommand(vilainNameQuery, dbConnection);
            command.Parameters.AddWithValue("@Id", id);
            var result = command.ExecuteScalar();

            string minionsQuery = "SELECT ROW_NUMBER() OVER (ORDER BY m.Name) as RowNum, " +
                                        "m.Name, " +
                                        "m.Age " +
                                        "FROM MinionsVillains AS mv " +
                                   "JOIN Minions As m ON mv.MinionId = m.Id " +
                                   "WHERE mv.VillainId = @Id " +
                                   "ORDER BY m.Name";

            if (result == null)
            {
                Console.WriteLine($"No villain with ID {id} exists in the database.");
            }
            else
            {
                Console.WriteLine($"Vilian: {result}");

                using (var minionsCommand = new SqlCommand(minionsQuery, dbConnection))
                {
                    minionsCommand.Parameters.AddWithValue("@Id", id);

                    using (var reader = minionsCommand.ExecuteReader())
                    {
                        if (!reader.HasRows)
                        {
                            Console.WriteLine("(no minions)");
                        }

                        while (reader.Read())
                        {
                            Console.WriteLine($"{reader[0]}. {reader[1]} {reader[2]}");
                        }
                    }
                }
            }
        }
    }
}
