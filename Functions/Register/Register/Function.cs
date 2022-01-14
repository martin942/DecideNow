using Google.Cloud.Functions.Framework;
using Microsoft.AspNetCore.Http;
using MySql.Data.MySqlClient;
using System;
using System.IO;
using System.Net.Mail;
using System.Text.Json;
using System.Threading.Tasks;

namespace Register
{
    public class Function : IHttpFunction
    {
        /// <summary>
        /// Logic for your function goes here.
        /// </summary>
        /// <param name="context">The HTTP context, containing the request and the response.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task HandleAsync(HttpContext context)
        {
            string method = context.Request.Method;
            if (method.Equals("POST"))
            {
                User userObject = await ParseJson(context.Request.Body);
                object validated = await ValidateUserData(userObject);
                if (!(validated is User))
                {
                    await SendResponseMessage(context, (Message)validated);
                    return;
                }

                int exist = await CheckIfUserExists(userObject);
                if (exist == 1)
                {
                    await SendResponseMessage(context, new Message("email already used", 400));
                    return;
                }
                else if (exist == 2)
                {
                    await SendResponseMessage(context, new Message("username already used", 400));
                    return;
                }
                await AddUser(userObject);
                await SendResponseMessage(context, new Message("user created", 200));
                return;
            }
        }

        private async Task<object> ValidateUserData(User user)
        {
            if (user.email == string.Empty || user.email == null || user.email == "")
            {
                return new Message("email can't be empty", 400);
            }

            if (!IsValidMail(user.email))
            {
                return new Message("invalid email address", 400);
            }

            if (user.userName == string.Empty || user.userName == null || user.userName == "")
            {
                return new Message("username can't be empty", 400);
            }

            if (user.password == string.Empty || user.password == null || user.password == "")
            {
                return new Message("password can't be empty", 400);
            }

            return user;
        }

        private async Task SendResponseMessage(HttpContext context, Message responseObject)
        {
            string response = JsonSerializer.Serialize(responseObject);
            Console.WriteLine(response);
            context.Response.ContentType = "application/json; charset=utf-8";
            context.Response.StatusCode = responseObject.statusCode;
            await context.Response.WriteAsync(response);
        }

        private async Task<User> ParseJson(Stream bodyStream)
        {
            User user = new User();
            using TextReader reader = new StreamReader(bodyStream);
            string text = await reader.ReadToEndAsync();
            if (text.Length > 0)
            {
                try
                {
                    JsonElement json = JsonSerializer.Deserialize<JsonElement>(text);
                    if (json.TryGetProperty("email", out JsonElement messageElementEmail) && messageElementEmail.ValueKind == JsonValueKind.String)
                    {
                        user.email = messageElementEmail.GetString();
                    }
                    if (json.TryGetProperty("password", out JsonElement messageElementPassword) && messageElementPassword.ValueKind == JsonValueKind.String)
                    {
                        user.password = messageElementPassword.GetString();
                    }
                    if (json.TryGetProperty("userName", out JsonElement messageElementUserName) && messageElementUserName.ValueKind == JsonValueKind.String)
                    {
                        user.userName = messageElementUserName.GetString();
                    }
                }
                catch (JsonException parseException)
                {
                    Console.WriteLine("something went wrong...");
                }
            }
            return user;
        }

        public static bool IsValidMail(string email)
        {
            try
            {
                MailAddress address = new MailAddress(email);
                return true;
            }
            catch (FormatException e)
            {
                return false;
            }
        }


        private async Task<bool> AddUser(User user)
        {
            MySqlConnection conn;
            MySqlTransaction transaction = null;
            try
            {
                conn = Sql.GetConnection();
                conn.Open();
                //if (conn.State != System.Data.ConnectionState.Open)
                //{
                //    conn.Open();
                //}
                string addUserCommandText = "insert into user(userid, username, email, createtime) values(@userid, @username, @email, @createtime)";
                string addPasswordCommandText = "insert into password(userid, password) values(@userid, @password)";
                transaction = conn.BeginTransaction();
                MySqlCommand addUserCommad = conn.CreateCommand();
                MySqlCommand addPassworCommand = conn.CreateCommand();
                addUserCommad.CommandText = addUserCommandText;
                addUserCommad.Connection = conn;
                addUserCommad.Transaction = transaction;
                addPassworCommand.CommandText = addPasswordCommandText;
                addPassworCommand.Connection = conn;
                addPassworCommand.Transaction = transaction;
                //todo:
                // set parameters
                string userId = Guid.NewGuid().ToString();
                addUserCommad.Parameters.Add("@userid", MySqlDbType.Text).Value = userId; //AddWithValue("@userid", userId);
                addUserCommad.Parameters.Add("@username", MySqlDbType.Text).Value = user.userName; //AddWithValue("@username", user.userName);
                addUserCommad.Parameters.Add("@email", MySqlDbType.Text).Value = user.email;  //AddWithValue("@email", user.email);
                addUserCommad.Parameters.Add("createtime", MySqlDbType.UInt64).Value = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();  //AddWithValue("@createtime", new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds());

                addPassworCommand.Parameters.AddWithValue("@userid", userId);
                addPassworCommand.Parameters.AddWithValue(@"password", user.password);

                await addUserCommad.ExecuteNonQueryAsync();
                await addPassworCommand.ExecuteNonQueryAsync();
                transaction.Commit();
                conn.Close();
            }
            catch
            {
                await transaction?.RollbackAsync();
                return false;
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <returns>
        /// 0 == user doesn't exist
        /// 1 == email already used
        /// 2 == username already used
        /// </returns>
        private async Task<int> CheckIfUserExists(User user)
        {
            using (MySqlConnection conn = Sql.GetConnection())
            {
                conn.Open();
                MySqlCommand checkEmailCommand = conn.CreateCommand();
                MySqlCommand checkUserNameCommand = conn.CreateCommand();
                checkEmailCommand.CommandText = $"select count(*) from user where email = n'{user.email}'";
                checkUserNameCommand.CommandText = $"select count(*) from user where username = n'{user.userName}'";
                long countEmail = (long)await checkEmailCommand.ExecuteScalarAsync();
                if (countEmail != 0)
                {
                    conn.Close();
                    return 1;
                }
                long countUserName = (long)await checkUserNameCommand.ExecuteScalarAsync();
                if (countUserName != 0)
                {
                    conn.Close();
                    return 2;
                }
                conn.Close();
                return 0;
            }
        }

        class Sql
        {
            private static MySqlConnection pMySqlConnection = null;

            public static MySqlConnection GetConnection()
            {
                if (pMySqlConnection == null)
                {
                    pMySqlConnection = new MySqlConnection(GetMySqlConnectionString().ConnectionString);
                }
                return new MySqlConnection(GetMySqlConnectionString().ConnectionString);
            }

            private static MySqlConnectionStringBuilder GetMySqlConnectionString()
            {
                MySqlConnectionStringBuilder connectionString;
                connectionString = NewMysqlTCPConnectionString();
                connectionString.MaximumPoolSize = 5;
                connectionString.MinimumPoolSize = 0;
                connectionString.ConnectionTimeout = 15;
                connectionString.ConnectionLifeTime = 1800;
                return connectionString;
            }

            private static MySqlConnectionStringBuilder NewMysqlTCPConnectionString()
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

    [Serializable]
    public class User
    {
        public string email;
        public string userName;
        public string password;

        public User()
        {

        }

        public User(string email, string userName, string password)
        {
            this.email = email;
            this.password = password;
            this.userName = userName;
        }

    }

    [Serializable]
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

}
