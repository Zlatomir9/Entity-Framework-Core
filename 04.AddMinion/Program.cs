using Microsoft.Data.SqlClient;
using System;

namespace _04.AddMinion
{
    class Program
    {
        static void Main(string[] args)
        {
            SqlConnection dbConnection = new SqlConnection(
                "Server=.\\SQLEXPRESS;DATABASE=MinionsDB;Integrated Security=true");
            dbConnection.Open();

            string[] minionInfo = Console.ReadLine().Split();
            string[] vilainInfo = Console.ReadLine().Split();

            string minionName = minionInfo[1];
            int age = int.Parse(minionInfo[2]);
            string cityName = minionInfo[3];
            string vilainName = vilainInfo[1];

            int? cityId = GetTownId(dbConnection, cityName);

            if (cityId == null)
            {
                string createCityQuery = "INSERT INTO Towns (Name) VALUES (@townName)";
                using var sqlCommand = new SqlCommand(createCityQuery, dbConnection);
                sqlCommand.Parameters.AddWithValue("@townName", cityName);
                sqlCommand.ExecuteNonQuery();
                cityId = GetTownId(dbConnection, cityName);
                Console.WriteLine($"Town {cityName} was added to the database.");
            }

            int? vilainId = GetVilainId(dbConnection, vilainName);

            if (vilainId == null)
            {
                string createVilain = "INSERT INTO Villains (Name, EvilnessFactorId)  VALUES (@villainName, 4)";
                using var sqlCommand = new SqlCommand(createVilain, dbConnection);
                sqlCommand.Parameters.AddWithValue("@villainName", vilainName);
                sqlCommand.ExecuteNonQuery();
                vilainId = GetVilainId(dbConnection, vilainName);
                Console.WriteLine($"Villain {vilainName} was added to the database.");
            }

            CreateMinion(dbConnection, minionName, age, cityId);
            var miniondId = GetMinionId(dbConnection, minionName);

            InsertMinionVilain(dbConnection, miniondId, vilainId);
            Console.WriteLine($"Successfully added {minionName} to be minion of {vilainName}.");
        }

        private static void InsertMinionVilain(SqlConnection dbConnection, int? miniondId, int? vilainId)
        {
            var insertIntoMinionsVil = "INSERT INTO MinionsVillains (MinionId, VillainId) VALUES (@minionId, @villainId)";

            var sqlCommand = new SqlCommand(insertIntoMinionsVil, dbConnection);
            sqlCommand.Parameters.AddWithValue("@villainId", vilainId);
            sqlCommand.Parameters.AddWithValue("@minionId", miniondId);
            sqlCommand.ExecuteNonQuery();
        }

        private static int? GetMinionId(SqlConnection dbConnection, string minionName)
        {
            var minionIdQuery = "SELECT Id FROM Minions WHERE Name = @Name";
            var sqlCommand = new SqlCommand(minionIdQuery, dbConnection);
            sqlCommand.Parameters.AddWithValue("@Name", minionName);
            var minionId = sqlCommand.ExecuteScalar();
            return (int?)minionId;
        }

        private static void CreateMinion(SqlConnection connection, string minionName, int age, int? cityId)
        {
            string createMinion = "INSERT INTO Minions (Name, Age, TownId) VALUES (@name, @age, @townId)";
            using var command = new SqlCommand(createMinion, connection);
            command.Parameters.AddWithValue("@name", minionName);
            command.Parameters.AddWithValue("@age", age);
            command.Parameters.AddWithValue("@townId", cityId);
            command.ExecuteNonQuery();
        }

        private static int? GetVilainId(
            SqlConnection connection, 
            string vilainName)
        {
            string vilainIdQuery = "SELECT Id FROM Villains WHERE Name = @Name";
            using var sqlCommand = new SqlCommand(vilainIdQuery, connection);
            sqlCommand.Parameters.AddWithValue("@Name", vilainName);
            var vilainId = sqlCommand.ExecuteScalar();

            return (int?)vilainId;
        }

        private static int? GetTownId(
            SqlConnection connection, 
            string cityName)
        {
            string cityIdQuery = "SELECT Id FROM Towns WHERE Name = @townName";
            using var sqlCommand = new SqlCommand(cityIdQuery, connection);
            sqlCommand.Parameters.AddWithValue("@townName", cityName);
            var cityId = sqlCommand.ExecuteScalar();

            return (int?)cityId;
        }
    }
}
