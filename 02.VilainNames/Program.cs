using Microsoft.Data.SqlClient;
using System;

namespace _02.Villain_Names
{
    class Program
    {
        static void Main(string[] args)
        {
            SqlConnection dbConnection = new SqlConnection(
                "Server=.\\SQLEXPRESS;DATABASE=MinionsDB;Integrated Security=true");

            dbConnection.Open();

            string query = "SELECT v.Name, COUNT(mv.VillainId) AS MinionsCount " +
                                "FROM Villains AS v " +
                                "JOIN MinionsVillains AS mv ON v.Id = mv.VillainId " +
                                "GROUP BY v.Id, v.Name " +
                                "HAVING COUNT(mv.VillainId) > 3 " +
                                "ORDER BY COUNT(mv.VillainId)";

            using (var command = new SqlCommand(query, dbConnection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while(reader.Read())
                    {
                        var name = reader[0];
                        var count = reader[1];

                        Console.WriteLine($"{name} - {count}");
                    }
                }
            }
        }
    }
}
