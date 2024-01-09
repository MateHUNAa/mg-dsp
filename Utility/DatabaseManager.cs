using MySql.Data.MySqlClient;
using System.Data;

namespace DiscordBotTemplateNet7.Utility
{

    public class DatabaseManager
    {
        private MySqlConnection _connection;
        private string _connectionString;

        public DatabaseManager(string connectionString)
        {
            _connectionString = connectionString;
            _connection = new MySqlConnection(_connectionString);
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
            MySqlDataReader reader = null;

            try
            {
                await connection.OpenAsync();
                reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection);
                return reader;
            } catch (Exception ex)
            {
                ConsoleColors.WriteLineWithColors($"[ ^4DatabaseManager ^0] [ ^1Error ^0] {ex}");
                await Program._logger.LogErrorAsync(ex);
                reader?.Dispose(); // Ensure the reader is disposed in case of an exception
                connection.Close(); // Manually close the connection
                return null;
            }
        }



    }
}
