using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Group
{
    public class SqlManager
    {
        private static SqlManager Instance;

        public async Task<bool> CreateGroup(Group group)
        {
            return false;
        }

        public async Task<bool> UpdateGroup(Group group)
        {
            return false;
        }

        public async Task<bool> DeleteGroup(Group group)
        {
            return false;
        }

        public async Task<string> GetGroupIdByName(string groupName)
        {
            return "";
        }

        public async Task<List<string>> GetGroupMembers(string groupid)
        {
            return null;
        }

        public async Task<bool> RemoveGroupMember(string userName)
        {
            return false;
        }

        //return userid
        public async Task<string> ValidateToken(string token)
        {
            return "";
        }


        public async Task CreatePersonGroup(int visibility, string groupname, string adminname)
        {
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand createGroup = conn.CreateCommand();
                createGroup.CommandText = $"Insert into PersonGroup (GroupId, Visibility, Name, Admin) values ('{Guid.NewGuid().ToString()}', {visibility}, '{groupname}','{adminname}')";
                createGroup.ExecuteNonQuery();
                conn.Close();
            }
        }

        public static SqlManager GetInstance()
        {
            if (Instance == null)
            {
                Instance = new SqlManager();
            }
            return Instance;
        }

        public MySqlConnection GetConnection()
        {
            return new MySqlConnection(GetMySqlConnectionString().ConnectionString);
        }

        private MySqlConnectionStringBuilder GetMySqlConnectionString()
        {
            MySqlConnectionStringBuilder connectionString;
            connectionString = NewMysqlTCPConnectionString();
            connectionString.MaximumPoolSize = 5;
            connectionString.MinimumPoolSize = 0;
            connectionString.ConnectionTimeout = 15;
            connectionString.ConnectionLifeTime = 1800;
            return connectionString;
        }

        private MySqlConnectionStringBuilder NewMysqlTCPConnectionString()
        {
            var connectionString = new MySqlConnectionStringBuilder()
            {
                SslMode = MySqlSslMode.None,
                Server = "34.79.128.207",
                UserID = "api",
                Password = "Passw0rd",
                Database = "dndb",
            };
            connectionString.Pooling = true;
            return connectionString;
        }

    }
}
