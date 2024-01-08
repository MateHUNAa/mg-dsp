using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public MySqlCommand CreateCommand(string query)
        {
            return new MySqlCommand(query, _connection);
        }
    }
}
