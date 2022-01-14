using Google.Cloud.Functions.Framework;
using Microsoft.AspNetCore.Http;
using MySql.Data.MySqlClient;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;

namespace Login
{
    public class Function : IHttpFunction
    {

        public async Task HandleAsync(HttpContext context)
        {
            string method = context.Request.Method;
            if (method.Equals("GET"))
            {
                await Get(context);
            }else if (method.Equals("POST"))
            {
                await Post(context);
            }else if (method.Equals("DELETE"))
            {
               await Delete(context);
            }
        }

        public async Task Get(HttpContext context)
        {
            string userName = ((string)context.Request.Query["userName"]);
            string email = ((string)context.Request.Query["email"]);

            string userId = await Sql.GetInstance().GetUserId(email, userName);
            if (userId == null || userId.Equals(""))
            {
                await SendResponseMessage(context, new Message("invalid email or username", 400));
                return;
            }
            (string, string) challenge = await Sql.GetInstance().GetChallenge(userId);
            await context.Response.WriteAsync(challenge.Item1 + "\n" + challenge.Item2);
        }

        public async Task Post(HttpContext context)
        {
            string userName = ((string)context.Request.Query["userName"]);
            string email = ((string)context.Request.Query["email"]);
            string challenge = ((string)context.Request.Query["challenge"]);

            string userId = await Sql.GetInstance().GetUserId(email, userName);

            if (userId == null || userId.Equals(""))
            {
                await SendResponseMessage(context, new Message("invalid email or username", 400));
                return;
            }

            if (challenge == null || challenge.Equals(""))
            {
                await SendResponseMessage(context, new Message("url must contain challenge", 400));
                return;
            }

            string solved = await new StreamReader(context.Request.Body).ReadToEndAsync();

            if (solved == null || solved.Equals(""))
            {
                await SendResponseMessage(context, new Message("enter password", 400));
                return;
            }

            string correctlySolved = await Sql.GetInstance().GetSolvedChallenge(userId, challenge);
            await Sql.GetInstance().RemoveChallenge(userId);
            if (solved.Equals(correctlySolved))
            {
                // create and send token
                Token token = Security.GenerateToken(userId, context.Connection.RemoteIpAddress.ToString());
                await Sql.GetInstance().AddToken(token);
                await context.Response.WriteAsync(token.tokenId);
            }
            else
            {
                await SendResponseMessage(context, new Message("wrong password", 400));
            }

        }


        // log out
        public async Task Delete(HttpContext context)
        {
            string token = ((string)context.Request.Query["token"]);
            string ip = ((string)context.Connection.RemoteIpAddress.ToString());
            int rows = await Sql.GetInstance().RemoveToken(token, ip);
            if (rows == 0)
            {
                await SendResponseMessage(context, new Message("failed to log out", 400));
                return;
            }else if (rows == 1)
            {
                await SendResponseMessage(context, new Message("log out successfull", 200));
                return;
            }
        }

        private async Task SendResponseMessage(HttpContext context, Message responseObject)
        {
            string response = JsonSerializer.Serialize(responseObject);
            Console.WriteLine(response);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = responseObject.statusCode;
            await context.Response.WriteAsync(response);
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

        public async Task AddToken(Token token)
        {
            MySqlConnection conn;
            MySqlTransaction transaction = null;

            try
            {
                conn = GetConnection();
                conn.Open();

                transaction = conn.BeginTransaction();

                MySqlCommand addTokenCommand = conn.CreateCommand();
                addTokenCommand.Connection = conn;
                addTokenCommand.Transaction = transaction;
                addTokenCommand.CommandText = $"insert into token(tokenid, userid, clientip, createtime, lastused) values(@tokenid, @userid, @clientip, @createtime, @lastused)";

                addTokenCommand.Parameters.Add("@tokenid", MySqlDbType.Text).Value = token.tokenId;
                addTokenCommand.Parameters.Add("@userid", MySqlDbType.Text).Value = token.userId;
                addTokenCommand.Parameters.Add("@clientip", MySqlDbType.Text).Value = token.clientIp;
                addTokenCommand.Parameters.Add("@createtime", MySqlDbType.UInt64).Value = token.createTime;
                addTokenCommand.Parameters.Add("@lastused", MySqlDbType.UInt64).Value = token.createTime;

                await addTokenCommand.ExecuteNonQueryAsync();
                await transaction.CommitAsync();
                await transaction.DisposeAsync();
                conn.Clone();
            }
            catch
            {
                await transaction.RollbackAsync();
            }
        }

        public async Task<int> RemoveToken(string token, string ip)
        {
            int rows = 0;
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand getUserIdCommand = conn.CreateCommand();
                getUserIdCommand.CommandText = $"delete from token where tokenid = '{token}' and clientip = '{ip}'";
                rows = await getUserIdCommand.ExecuteNonQueryAsync();
                conn.Close();
            }
            return rows;
        }

        public async Task RemoveChallenge(string userid)
        {
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand getUserIdCommand = conn.CreateCommand();
                getUserIdCommand.CommandText = $"delete from challenge where userid = '{userid}'";
                await getUserIdCommand.ExecuteNonQueryAsync();
                conn.Close();
            }
        }

        public async Task<string> GetSolvedChallenge(string userid, string challenge)
        {
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand getUserIdCommand = conn.CreateCommand();
                getUserIdCommand.CommandText = $"select solved from challenge where userid = '{userid}' and challenge = '{challenge}'";
                string solved = (string)await getUserIdCommand.ExecuteScalarAsync();
                conn.Close();
                return solved;
            }
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

        public async Task AddChallenge(Challenge challenge)
        {
            MySqlConnection conn;
            MySqlTransaction transaction = null;

            try
            {
                conn = GetConnection();
                conn.Open();

                transaction = conn.BeginTransaction();

                MySqlCommand addChallengeCommand = conn.CreateCommand();
                addChallengeCommand.Connection = conn;
                addChallengeCommand.Transaction = transaction;
                addChallengeCommand.CommandText = $"insert into challenge(userid, challenge, solved, createtime) values(@userid, @challenge, @solved, @createtime)";

                addChallengeCommand.Parameters.Add("@userid", MySqlDbType.Text).Value = challenge.userId;
                addChallengeCommand.Parameters.Add("@challenge", MySqlDbType.Text).Value = challenge.challenge;
                addChallengeCommand.Parameters.Add("@solved", MySqlDbType.Text).Value = challenge.solved;
                addChallengeCommand.Parameters.Add("@createtime", MySqlDbType.UInt64).Value = challenge.createTime;

                await addChallengeCommand.ExecuteNonQueryAsync();
                await transaction.CommitAsync();
                await transaction.DisposeAsync();
                conn.Clone();
            }
            catch
            {
                await transaction.RollbackAsync();
            }
        }

        public async Task<(string, string)> GetChallenge(string userId)
        {
            Challenge challenge = new Challenge(userId);
            await AddChallenge(challenge);
            return (challenge.challenge, challenge.solved);
        }

        public string GetPassword(string userid)
        {
            string password;
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand addChallengeCommand = conn.CreateCommand();
                addChallengeCommand.CommandText = $"select password from password where userid = '{userid}'";
                password = (string)addChallengeCommand.ExecuteScalar();
                conn.Close();
                return password;
            }
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

    public class Security
    {
        public static Token GenerateToken(string userId, string clientIp)
        {
            return new Token(userId, clientIp);
        }

        public static string EncriptChallenge(string challenge, string password)
        {
            byte[] challegeBytes = Convert.FromBase64String(challenge);
            byte[] passwordBytes = Convert.FromBase64String(password);
            HashAlgorithm alogirith = new SHA256Managed();
            byte[] challengeWithPass = new byte[challegeBytes.Length + passwordBytes.Length];
            for (int i = 0; i < challegeBytes.Length; i++)
            {
                challengeWithPass[i] = challegeBytes[i];
            }
            for (int i = 0; i < passwordBytes.Length; i++)
            {
                challengeWithPass[i + challegeBytes.Length] = passwordBytes[i];
            }

            return Convert.ToBase64String(alogirith.ComputeHash(challengeWithPass));
        }

    }

    public class Message
    {
        public string message { get; set; }
        public int statusCode { get; set; }

        public Message(string message, int statusCode)
        {
            this.message = message;
            this.statusCode = statusCode;
        }
    }

    public class Token
    {
        public string tokenId;
        public string userId;
        public string clientIp;
        public long createTime;
        public long lastUsed;

        public Token(string userId, string clientIp)
        {
            this.tokenId = Guid.NewGuid().ToString();
            this.userId = userId;
            this.clientIp = clientIp;
            this.createTime = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
            this.lastUsed = createTime;
        }
    }

    public class Challenge
    {
        public string userId { get; set; }
        public string challenge { get; set; }
        public string solved { get; set; }
        public long createTime { get; set; }

        public Challenge(string userId)
        {
            this.userId = userId;
            this.challenge = Generate();
            this.solved = Solve(challenge, GetPassword());
            this.createTime = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
        }

        private string Generate()
        {
            byte[] challenge = new byte[32];
            new Random().NextBytes(challenge);
            return Convert.ToBase64String(challenge);
        }

        private string Solve(string challenge, string password)
        {
            string solved = Security.EncriptChallenge(challenge, password);
            Console.WriteLine(solved);
            return solved;
        }

        private string GetPassword()
        {
            string password = Sql.GetInstance().GetPassword(userId);
            return password;
        }

    }

}
