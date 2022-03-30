using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Follow
{
    public class Sql
    {

        private static Sql instance = null;


        public async Task<string> GetUserIdByToken(string token, string ip)
        {
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand getUserIdCommand = conn.CreateCommand();
                getUserIdCommand.CommandText = $"select userid from token where tokenid = '{token}' and clientip = '{ip}'";
                string userId = (string)await getUserIdCommand.ExecuteScalarAsync();
                conn.Close();
                return userId;
            }
        }

        public async Task<string> GetUsernameByUserId(string userid)
        {
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand getUserIdCommand = conn.CreateCommand();
                getUserIdCommand.CommandText = $"select username from user where userid = '{userid}'";
                string userId = (string)await getUserIdCommand.ExecuteScalarAsync();
                conn.Close();
                return userId;
            }
        }

        public async Task<string> GetUserIdByUsername(string userName)
        {
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand getUserIdCommand = conn.CreateCommand();
                getUserIdCommand.CommandText = $"select userid from user where username = '{userName}'";
                string userId = (string)await getUserIdCommand.ExecuteScalarAsync();
                conn.Close();
                return userId;
            }
        }

        public async Task<int> FollowUser(string from, string to)
        {
            if (await IsFollowing(from, to))
            {
                return 1;
            }
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand followCommand = conn.CreateCommand();
                followCommand.CommandText = $"insert into follow (fromid, toid, totype) values ('{from}', '{to}', 'user')";
                int rows = await followCommand.ExecuteNonQueryAsync();
                Console.WriteLine(rows);
                conn.Close();
                return rows;
            }
        }

        public async Task<int> UnfollowUser(string from, string to)
        {
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand unfollowCommand = conn.CreateCommand();
                unfollowCommand.CommandText = $"delete from follow where fromid = '{from}' and toid = '{to}'";
                int rows = await unfollowCommand.ExecuteNonQueryAsync();
                conn.Close();
                return rows;
            }
        }

        public async Task<bool> IsFollowing(string userid, string isFollowingUserId)
        {
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand getUserIdCommand = conn.CreateCommand();
                getUserIdCommand.CommandText = $"select count(*) from follow where fromid = '{userid}' and toid = '{isFollowingUserId}'";
                int count = (int)((long)await getUserIdCommand.ExecuteScalarAsync());
                conn.Close();
                return count==1;
            }
        }

        public async Task<List<string>> Following(string userId)
        {
            List<string> following = new List<string>();
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand getFollowingCommand = conn.CreateCommand();
                getFollowingCommand.CommandText = $"select user.username from follow inner join user on follow.toid = user.userid where fromid = '{userId}'";
                MySqlDataReader reader = getFollowingCommand.ExecuteReader();
                while (reader.Read())
                {
                    following.Add(reader.GetString(0));
                }
                conn.Close();
            }
            return following;
        }

        public async Task<List<string>> Followers(string userId)
        {
            List<string> followers = new List<string>();
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand getFollowersCommand = conn.CreateCommand();
                getFollowersCommand.CommandText = $"select user.username from follow inner join user on follow.fromid = user.userid where toid = '{userId}'";
                MySqlDataReader reader = getFollowersCommand.ExecuteReader();
                while (reader.Read())
                {
                    followers.Add(reader.GetString(0));
                }
                conn.Close();
            }
            return followers;
        }

        public async Task<int> FollowingCount(string userId)
        {
            int count = -1;
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand getFollowingCountCommand = conn.CreateCommand();
                getFollowingCountCommand.CommandText = $"select count(*) from follow where fromid = '{userId}'";
                count = (int)(long)await getFollowingCountCommand.ExecuteScalarAsync();
                conn.Close();
            }
            return count;
        }

        public async Task<int> FollowersCount(string userId)
        {
            int count = -1;
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand getFollowersCountCommand = conn.CreateCommand();
                getFollowersCountCommand.CommandText = $"select count(*) from follow where toid = '{userId}'";
                count = (int)(long)await getFollowersCountCommand.ExecuteScalarAsync();
                conn.Close();
            }
            return count;
        }

        public static Sql GetInstance()
        {
            if (instance == null)
            {
                instance = new Sql();
            }
            return instance;
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
    }
}
