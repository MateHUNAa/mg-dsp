using DSharpPlus.Entities;
using MySql.Data.MySqlClient;
using System.Data;

namespace DiscordBotTemplateNet7.Utility
{

    public class DatabaseManager
    {
        public MySqlConnection _connection;
        private string _connectionString;

        public DatabaseManager(string connectionString)
        {
            _connectionString = connectionString;
            _connection = new MySqlConnection(_connectionString);
            ConsoleColors.WriteLineWithColors("[ ^4DatabaseManager ^0] [ ^3Info ^0] Database Connected!");
            string[] channels =
            {
                "global"
            };
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder
            {
                Title = "Database Manager",
                Description = "The DatabaseManager module establishes a stable connection with the database.",
                Color = DiscordColor.Green
            };
            Program._logger.SendEmbed(embed, channels);
        }

        public void OpenConnection()
        {
            if (_connection.State == System.Data.ConnectionState.Closed)
            {
                _connection.Open();
            }
        }

        public void CloseConnection()
        {
            if (_connection.State == System.Data.ConnectionState.Open)
            {
                _connection.Close();
            }
        }

        public async Task OpenConnectionAsync()
        {
            if (_connection.State == System.Data.ConnectionState.Closed)
            {
                await _connection.OpenAsync();
            }
        }

        public async Task CloseConnectionAsync()
        {
            if (_connection.State == System.Data.ConnectionState.Open)
            {
                _connection.Close();
            }
        }


        public MySqlCommand CreateCommand(string query)
        {
            return new MySqlCommand(query, _connection);
        }


        public async Task<MySqlDataReader> ExecuteReaderAsync(string query)
        {
            MySqlConnection connection = new MySqlConnection(_connectionString);
            MySqlCommand command = new MySqlCommand(query, connection);

            try
            {
                await connection.OpenAsync();
                MySqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection);
                return reader;
            } catch (Exception ex)
            {
                ConsoleColors.WriteLineWithColors($"[ ^4DatabaseManager ^0] [ ^1Error ^0] {ex}");
                await Program._logger.LogErrorAsync(ex);
                connection.Close(); // Manually close the connection in case of an exception
                return null;
            }
        }


        public async Task<MySqlDataReader> ExecuteReaderWithParametersAsync(string query, Dictionary<string, object> parameters)
        {
            MySqlConnection connection = new MySqlConnection(_connectionString);
            MySqlCommand command = new MySqlCommand(query, connection);

            if (parameters != null)
            {
                foreach (var parameter in parameters)
                {
                    Console.WriteLine(parameter.Key);
                    command.Parameters.AddWithValue(parameter.Key, parameter.Value);
                }
                parameters.Clear();
            }

            try
            {
                await connection.OpenAsync();
                MySqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection);
                return reader;
            } catch (Exception ex)
            {
                ConsoleColors.WriteLineWithColors($"[ ^4DatabaseManager ^0] [ ^1Error ^0] {ex}");
                await Program._logger.LogErrorAsync(ex);
                connection.Close(); // Manually close the connection in case of an exception
                return null;
            } 
        }

    }
}
