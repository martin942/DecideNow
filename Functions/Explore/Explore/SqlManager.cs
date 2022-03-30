using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Explore
{
    public class SqlManager
    {
        private static SqlManager instance;


        public async Task<List<string>> GetUsers(string searchValue, string username = "", string userid = "")
        {
            List<string> users = new List<string>();
            List<string> temp_users = new List<string>();

            string getExactUsernameCommandString = $"select username from user where username = '{searchValue}' and username != '{username}' and userid != '{userid}'";
            string getSimilarUsernameCommandString = $"select username from user where (username like '{searchValue}%' or username like '%{searchValue}' or username like '%{searchValue}%') and username != '{username}' and userid != '{userid}' order by username asc";

            using(MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand getExactUsernameCommand = conn.CreateCommand();
                getExactUsernameCommand.CommandText = getExactUsernameCommandString;
                getExactUsernameCommand.Connection = conn;
                string name = (string) await getExactUsernameCommand.ExecuteScalarAsync();
                if (name != null)
                {
                    users.Add(name);
                }

                MySqlCommand getSimilarUsernameCommand = conn.CreateCommand();
                getSimilarUsernameCommand.CommandText= getSimilarUsernameCommandString;
                getSimilarUsernameCommand.Connection= conn;
                MySqlDataReader reader = (MySqlDataReader) await getSimilarUsernameCommand.ExecuteReaderAsync();
                while (reader.Read())
                {
                    temp_users.Add(reader.GetString(0));
                }
                reader.Close();
                conn.Close();
            }
            if (users.Count > 0)
            {
                if (temp_users.Contains(users[0]))
                {
                    temp_users.Remove(users[0]);
                }
            }
            users.AddRange(temp_users);

            return users;

        }



        public MySqlConnection GetConnection()
        {
            return new MySqlConnection(GetMySqlConnectionString().ConnectionString);
        }

        private MySqlConnectionStringBuilder GetMySqlConnectionString()
        {
            var connectionString = new MySqlConnectionStringBuilder()
            {
                SslMode = MySqlSslMode.None,
                Server = "34.78.100.58",
                UserID = "api",
                Password = "Passw0rd",
                Database = "dndb",
            };
            connectionString.Pooling = true;
            connectionString.MaximumPoolSize = 5;
            connectionString.MinimumPoolSize = 0;
            connectionString.ConnectionTimeout = 15;
            connectionString.ConnectionLifeTime = 1800;
            return connectionString;
        }

        public static SqlManager GetInstance()
        {
            if(instance == null)
            {
                instance = new SqlManager();
            }
            return instance;
        }
    }
}
