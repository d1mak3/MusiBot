using Newtonsoft.Json.Linq;
using MySqlConnector;
using MusiBotProd.Utilities.Data.Models;

namespace MusiBotProd.Utilities.Data.DataProviders
{
    /// <summary>
    /// Provider to interact with mysql
    /// </summary>
    public class MySqlDataProvider : IDataProvider
    {
        #region readonlies

        private readonly MySqlConnection databaseConnection;

        #endregion

        #region constructors

        public MySqlDataProvider(string connectionString)
        {
            for (int i = 0; i < Convert.ToInt32(Environment.GetEnvironmentVariable("RETRIES_COUNT")); ++i)
            {
                try
                {
                    databaseConnection = new MySqlConnection(connectionString);
                    databaseConnection.Open();                    
                    break;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Thread.Sleep(5000);
                }                
            }            
        }

        #endregion

        #region supporting methods

        public void OpenConnection()
        {
            databaseConnection.Open();
        }

        public List<JObject> Get(string dbName, int dbColumnsCount)
        {
            MySqlCommand command = new MySqlCommand($"SELECT * FROM {dbName};", databaseConnection);
            
            List<JObject> outputList = new List<JObject>();

            using (MySqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    JObject newData = new JObject();
                    bool errorFound = true;

                    for (int i = 0; i < dbColumnsCount; ++i)
                    {
                        try
                        {
                            newData[$"{i}"] = reader.GetInt32(i);
                            errorFound = false;
                            continue;
                        }
                        catch
                        {                            
                        }

                        try
                        {
                            newData[$"{i}"] = reader.GetString(i);
                            errorFound = false;
                            continue;
                        }
                        catch
                        {
                        }

                        try
                        {
                            newData[$"{i}"] = reader.GetDateTime(i);
                            errorFound = false;
                            continue;
                        }
                        catch
                        {
                            errorFound = true;
                            break;
                        }                        
                    }

                    if (!errorFound)
                        outputList.Add(newData);
                }
            }

            return outputList;
        }               

        public void SaveBalance(Balance balance)
        {    
            using (var command = new MySqlCommand())
            {
                command.Connection = databaseConnection;

                command.CommandText = $"SELECT * FROM balance WHERE id = {balance.Id}";

                using (var reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        reader.Close();

                        command.CommandText = $"UPDATE balance " +
                                              $"SET id_of_user_and_guild_connection = @connection," +
                                                  $"amount_of_coins_on_deposit = @dep," +
                                                  $"amount_of_coins_in_cash = @cash " +
                                              $"WHERE id = @id;";
                        command.Parameters.AddWithValue("id", balance.Id);
                        command.Parameters.AddWithValue("connection", balance.IdOfUnGConnection);
                        command.Parameters.AddWithValue("dep", balance.AmountOfCoinsOnDeposit);
                        command.Parameters.AddWithValue("cash", balance.AmountOfCoinsInCash);
                        command.ExecuteNonQuery();

                        return;
                    }
                }

                command.CommandText = $"INSERT INTO balance VALUES (@id, @connection, @dep, @cash);";
                command.Parameters.AddWithValue("id", balance.Id);
                command.Parameters.AddWithValue("connection", balance.IdOfUnGConnection);
                command.Parameters.AddWithValue("dep", balance.AmountOfCoinsOnDeposit);
                command.Parameters.AddWithValue("cash", balance.AmountOfCoinsInCash);
                command.ExecuteNonQuery();
            }
        }

        public void SaveGuild(Guild guild)
        {
            using (var command = new MySqlCommand())
            {
                command.Connection = databaseConnection;

                command.CommandText = $"SELECT * FROM guild WHERE id = {guild.Id};";

                using (var reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        return;
                    }
                }

                command.CommandText = $"INSERT INTO guild VALUES (@id);";
                command.Parameters.AddWithValue("id", guild.Id);                
                command.ExecuteNonQuery();
            }
        }

        public void SaveRnGConnection(RoleAndGuildConnection connection)
        {
            using (var command = new MySqlCommand())
            {
                command.Connection = databaseConnection;

                command.CommandText = $"SELECT * FROM role_and_guild_connection WHERE id = {connection.Id};";

                using (var reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        reader.Close();

                        command.CommandText = $"UPDATE role_and_guild_connection " +
                                              $"SET guild_id = @guild," +
                                                  $"role_name = @rname," +
                                                  $"income = @income " +
                                              $"WHERE id = @id;";
                        command.Parameters.AddWithValue("id", connection.Id);
                        command.Parameters.AddWithValue("guild", connection.GuildId);
                        command.Parameters.AddWithValue("rname", connection.RoleName);
                        command.Parameters.AddWithValue("income", connection.Income);
                        command.ExecuteNonQuery();

                        return;
                    }
                }

                command.CommandText = $"INSERT INTO role_and_guild_connection VALUES (@id, @guild, @rname, @income);";
                command.Parameters.AddWithValue("id", connection.Id);
                command.Parameters.AddWithValue("guild", connection.GuildId);
                command.Parameters.AddWithValue("rname", connection.RoleName);
                command.Parameters.AddWithValue("income", connection.Income);
                command.ExecuteNonQuery();
            }
        }

        public void SaveUser(User user)
        {
            using (var command = new MySqlCommand())
            {
                command.Connection = databaseConnection;

                command.CommandText = $"SELECT * FROM user WHERE id = {user.Id}";

                using (var reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        return;
                    }
                }

                command.CommandText = $"INSERT INTO user VALUES (@id);";
                command.Parameters.AddWithValue("id", user.Id);
                command.ExecuteNonQuery();
            }
        }

        public void SaveUnGConnection(UserAndGuildConnection connection)
        {
            using (var command = new MySqlCommand())
            {
                command.Connection = databaseConnection;

                command.CommandText = $"SELECT * FROM user_and_guild_connection WHERE id = {connection.Id}";

                using (var reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        reader.Close();

                        command.CommandText = $"UPDATE user_and_guild_connection " +
                                              $"SET user_id = @user," +
                                                  $"guild_id = @guild," +
                                                  $"earn_time = @earnt," +
                                                  $"role_income_time = @rit " +
                                              $"WHERE id = @id;";
                        command.Parameters.AddWithValue("id", connection.Id);
                        command.Parameters.AddWithValue("user", connection.UserId);
                        command.Parameters.AddWithValue("guild", connection.GuildId);
                        command.Parameters.AddWithValue("earnt", connection.EarnTime);
                        command.Parameters.AddWithValue("rit", connection.RoleIncomeTime);
                        command.ExecuteNonQuery();

                        return;
                    }
                }

                command.CommandText = $"INSERT INTO user_and_guild_connection VALUES (@id, @user, @guild, @earnt, @rit);";
                command.Parameters.AddWithValue("id", connection.Id);
                command.Parameters.AddWithValue("user", connection.UserId);
                command.Parameters.AddWithValue("guild", connection.GuildId);
                command.Parameters.AddWithValue("earnt", connection.EarnTime);
                command.Parameters.AddWithValue("rit", connection.RoleIncomeTime);
                command.ExecuteNonQuery();
            }
        }

        #endregion
    }
}
