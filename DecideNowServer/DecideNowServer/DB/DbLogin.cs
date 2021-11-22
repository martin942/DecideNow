using DecideNowServer.Exceptions;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace DecideNowServer.DB
{
    public class DbLogin : DbBase
    {

        private static DbLogin instance = null;

        private DbLogin()
        {

        }


        public async Task<string> getUserIdByEmail(string email)
        {
            string getUserIdCommandString = "select id from user where email = @email";
            string userId = "";

            MySqlTransaction getUserIdTransaction = null;

            try
            {
                Connect();

                MySqlCommand getUserIdCommand = new MySqlCommand(getUserIdCommandString, GetConnection());
                getUserIdTransaction = GetConnection().BeginTransaction();

                getUserIdCommand.Transaction = getUserIdTransaction;

                getUserIdCommand.Parameters.Add("@email", MySqlDbType.Text).Value = email;

                MySqlDataReader reader = getUserIdCommand.ExecuteReader();

                while (reader.Read())
                {
                    userId = reader.GetString(0);
                }

                Disconnect();
            }
            catch (InvalidOperationException ex)
            {
                try
                {
                    await getUserIdTransaction.RollbackAsync();
                    throw new InternalServerException("Somethiong went wrong");
                }
                catch (MySqlException e)
                {
                    throw new InternalServerException("Somethiong went very wrong 1");
                }
            }
            catch (MySqlException ex)
            {
                try
                {
                    await getUserIdTransaction.RollbackAsync();
                    throw new InternalServerException("Somethiong went wrong");
                }
                catch (MySqlException e)
                {
                    Console.WriteLine(ex.Message);
                    throw new InternalServerException("Somethiong went very wrong 2");
                }
            }
            return userId;
        }

        public async Task<string> GetPasswordByUserId(string userId)
        {
            string getPasswordCommandString = "select password from user where id = @userId";
            string password = "";

            MySqlTransaction getPasswordTransaction = null;

            try
            {
                Connect();

                MySqlCommand getPasswordCommand = new MySqlCommand(getPasswordCommandString, GetConnection());
                getPasswordTransaction = GetConnection().BeginTransaction();

                getPasswordCommand.Transaction = getPasswordTransaction;

                getPasswordCommand.Parameters.Add("@userId", MySqlDbType.Text).Value = userId;

                MySqlDataReader reader = getPasswordCommand.ExecuteReader();

                while (reader.Read())
                {
                    password = reader.GetString(0);
                }

                Disconnect();
            }
            catch (InvalidOperationException ex)
            {
                try
                {
                    await getPasswordTransaction.RollbackAsync();
                    throw new InternalServerException("Somethiong went wrong");
                }
                catch (MySqlException e)
                {
                    throw new InternalServerException("Somethiong went very wrong 3");
                }
            }
            catch (MySqlException ex)
            {
                try
                {
                    await getPasswordTransaction.RollbackAsync();
                    throw new InternalServerException("Somethiong went wrong");
                }
                catch (MySqlException e)
                {
                    throw new InternalServerException("Somethiong went very wrong 4");
                }
            }
            return password;
        }


        public async Task<int> SetLastLogin(string userId)
        {
            string setLastLoginCommandString = "update user set last_login_date = @lastlogindate where id = @id";
            int rows = 0;
            MySqlTransaction setLastLOginTransaction = null;

            try
            {
                Connect();

                MySqlCommand setLastLoginCommand = new MySqlCommand(setLastLoginCommandString, GetConnection());
                setLastLOginTransaction = GetConnection().BeginTransaction();

                setLastLoginCommand.Transaction = setLastLOginTransaction;

                setLastLoginCommand.Parameters.Add("@id", MySqlDbType.Text).Value = userId;
                setLastLoginCommand.Parameters.Add("@lastlogindate", MySqlDbType.Int64).Value = ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds();
                rows = await setLastLoginCommand.ExecuteNonQueryAsync();

                await setLastLOginTransaction.CommitAsync();
            }
            catch (InvalidOperationException ex)
            {
                try
                {
                    await setLastLOginTransaction.RollbackAsync();
                    throw new InternalServerException("Somethiong went wrong");
                }
                catch (MySqlException e)
                {
                    throw new InternalServerException("Somethiong went very wrong");
                }
            }
            catch (MySqlException ex)
            {
                try
                {
                    await setLastLOginTransaction.RollbackAsync();
                    throw new InternalServerException("Somethiong went wrong");
                }
                catch (MySqlException e)
                {
                    throw new InternalServerException("Somethiong went very wrong");
                }
            }

            Disconnect();

            return rows;
        } 


        public static DbLogin GetInstance()
        {
            if (instance == null)
            {
                instance = new DbLogin();
            }
            return instance;
        }

    }
}
