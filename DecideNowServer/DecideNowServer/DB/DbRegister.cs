using DecideNowServer.Exceptions;
using DecideNowServer.Security;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace DecideNowServer.DB
{
    public class DbRegister : DbBase
    {

        private static DbRegister instance = null;

        public DbRegister()
        {
        }


        public async Task<int> AddUser(RegisterModel userModel)
        {
            string addUserCommandString = "insert into user(id, user_name, first_name, last_name, email, birth_date, password, create_date) values(@id, @username, @firstname, @lastname, @email, @birthdate, @passwordhash, @createdate)";
            int rows = 0;
            MySqlTransaction addUserTransaction = null;

            try
            {
                Connect();

                MySqlCommand addUserCommand = new MySqlCommand(addUserCommandString, GetConnection());
                addUserTransaction = GetConnection().BeginTransaction();

                addUserCommand.Transaction = addUserTransaction;

                addUserCommand.Parameters.Add("@id", MySqlDbType.Text).Value = Guid.NewGuid().ToString();
                addUserCommand.Parameters.Add("@username", MySqlDbType.Text).Value = userModel.user_name;
                addUserCommand.Parameters.Add("@firstname", MySqlDbType.Text).Value = userModel.first_name;
                addUserCommand.Parameters.Add("@lastname", MySqlDbType.Text).Value = userModel.last_name;
                addUserCommand.Parameters.Add("@email", MySqlDbType.Text).Value = userModel.email;
                addUserCommand.Parameters.Add("@birthdate", MySqlDbType.Text).Value = userModel.birth_date;
                addUserCommand.Parameters.Add("@passwordhash", MySqlDbType.Text).Value = userModel.password_hash;
                addUserCommand.Parameters.Add("@createdate", MySqlDbType.Int64).Value = ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds();
                rows = await addUserCommand.ExecuteNonQueryAsync();

                await addUserTransaction.CommitAsync();
            }
            catch (InvalidOperationException ex)
            {
                try
                {
                    await addUserTransaction.RollbackAsync();
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
                    await addUserTransaction.RollbackAsync();
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


        public static DbRegister GetInstance()
        {
            if (instance == null)
            {
                return new DbRegister();
            }
            return instance;
        }

    }
}
