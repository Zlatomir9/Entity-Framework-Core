using Microsoft.Data.SqlClient;
using System;
using System.Linq;

namespace _08.IncreaseMinionAge
{
    class Program
    {
        static void Main(string[] args)
        {
            SqlConnection dbConnection = new SqlConnection(
                "Server=.\\SQLEXPRESS;DATABASE=MinionsDB;Integrated Security=true");
            dbConnection.Open();

            int[] minionsId = Console.ReadLine().Split().Select(int.Parse).ToArray();
            string updateMinionsQuery = "UPDATE Minions " +
                                        "SET Name = UPPER(LEFT(Name, 1)) + SUBSTRING(Name, 2, LEN(Name)), Age += 1 " +
                                        "WHERE Id = @Id";

            foreach (var id in minionsId)
            {
                using var sqlCommand = new SqlCommand(updateMinionsQuery, dbConnection);
                sqlCommand.Parameters.AddWithValue("@Id", id);
                sqlCommand.ExecuteNonQuery();
            }


            var selectMinions = "SELECT Name, Age FROM Minions";

            using var selectCommand = new SqlCommand(selectMinions, dbConnection);
            using var reader = selectCommand.ExecuteReader();

            while (reader.Read())
            {
                Console.WriteLine($"{reader[0]} {reader[1]}");
            }
        }
    }
}
