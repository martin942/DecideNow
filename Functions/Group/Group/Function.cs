using Google.Cloud.Functions.Framework;
using Microsoft.AspNetCore.Http;
using MySql.Data.MySqlClient;
using System;
using System.Threading.Tasks;

namespace Group
{
    public class Function : IHttpFunction
    {
        
        public async Task HandleAsync(HttpContext context)
        {
            await context.Response.WriteAsync("Hello, Functions Framework.");
        }
    }
    public class Sql
    {
        private static Sql Instance;

        public static Sql GetInstance()
        {
            if (Instance == null)
            {
                Instance = new Sql();
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
        public async Task<string> GetUserId(string email, string userName)
        {
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand getUserIdCommand = conn.CreateCommand();
                getUserIdCommand.CommandText = $"select userid from user where username = '{userName}' or email = '{email}'";
                string userId = (string)await getUserIdCommand.ExecuteScalarAsync();
                conn.Close();
                return userId;
            }
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


    }
}
