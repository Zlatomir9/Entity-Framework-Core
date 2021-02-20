using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;

namespace _05.ChangeTownNamesCasing
{
    class Program
    {
        static void Main(string[] args)
        {
            SqlConnection dbConnection = new SqlConnection(
                "Server=.\\SQLEXPRESS;DATABASE=MinionsDB;Integrated Security=true");
            dbConnection.Open();

            string input = Console.ReadLine();

            string updateTownNameQuery = "UPDATE Towns " +
                                         "SET Name = UPPER(Name) " +
                                         "WHERE CountryCode = (SELECT c.Id FROM Countries AS c " +
                                                                                "WHERE c.Name = @countryName)";

            string selectTownNamesQuery = "SELECT t.Name " +
                                          "FROM Towns as t " +
                                          "JOIN Countries AS c ON c.Id = t.CountryCode " +
                                          "WHERE c.Name = @countryName";

            using var updateCommand = new SqlCommand(updateTownNameQuery, dbConnection);
            updateCommand.Parameters.AddWithValue("@countryName", input);
            var affectedRows = updateCommand.ExecuteNonQuery();

            if (affectedRows == 0)
            {
                Console.WriteLine("No town names were affected.");
            }
            else
            {
                Console.WriteLine($"{affectedRows} town names were affected.");

                using var selectCommand = new SqlCommand(selectTownNamesQuery, dbConnection);
                selectCommand.Parameters.AddWithValue("@countryName", input);

                using (var reader = selectCommand.ExecuteReader())
                {
                    var towns = new List<string>();

                    while (reader.Read())
                    {
                        towns.Add((string)reader[0]);
                    }

                    Console.WriteLine($"[{string.Join(", ", towns)}]");
                }
            }
        }
    }
}
