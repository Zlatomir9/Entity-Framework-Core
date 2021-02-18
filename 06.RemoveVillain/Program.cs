using Microsoft.Data.SqlClient;
using System;

namespace _06.RemoveVillain
{
    class Program
    {
        static void Main(string[] args)
        {
            SqlConnection dbConnection = new SqlConnection(
                "Server=.\\SQLEXPRESS;DATABASE=MinionsDB;Integrated Security=true");
            dbConnection.Open();

            int value = int.Parse(Console.ReadLine());

            string evilNameQuery = "SELECT Name FROM Villains WHERE Id = @villainId";
            using var sqlCommand = new SqlCommand(evilNameQuery, dbConnection);
            sqlCommand.Parameters.AddWithValue("@villainId", value);
            var name = (string)sqlCommand.ExecuteScalar();

            if (name == null)
            {
                Console.WriteLine("No such villain was found.");
                return;
            }

            var deleteMinionsVillainsQuery = "DELETE FROM MinionsVillains " +
                                             "WHERE VillainId = @villainId";

            using var sqlDeleteMVCommand = new SqlCommand(deleteMinionsVillainsQuery, dbConnection);
            sqlDeleteMVCommand.Parameters.AddWithValue("@villainId", value);
            var affectedRows = sqlDeleteMVCommand.ExecuteNonQuery();

            var deleteVillainQuery = "DELETE FROM Villains " +
                                      "WHERE Id = @villainId";

            using var sqlDeleteVillainCommand = new SqlCommand(deleteVillainQuery, dbConnection);
            sqlDeleteVillainCommand.Parameters.AddWithValue("@villainId", value);
            sqlDeleteVillainCommand.ExecuteNonQuery();

            Console.WriteLine($"{name} was deleted.");
            Console.WriteLine($"{affectedRows} minions were released.");
        }
    }
}
