using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DecideNowServer.DB
{
    public class DbBase
    {

        public static string connectionString = "Server=localhost;Uid=root;Pwd=Passw0rd;Database=decidenowdb";

        public MySqlConnection sqlConnection { get; private set; }

        public static MySqlConnection connection;

        public static MySqlConnection GetConnection()
        {
            if (connection == null)
            {
                connection = new MySqlConnection(connectionString);
            }
            return connection;
        }


        public bool Connect()
        {
            if (GetConnection().State != System.Data.ConnectionState.Open)
            {
                GetConnection().Open();
                return true;
            }
            if (GetConnection().State == System.Data.ConnectionState.Open)
            {
                return true;
            }

            return false;
        }

        public bool Disconnect()
        {
            if (GetConnection().State == System.Data.ConnectionState.Open)
            {
                GetConnection().Close();
                return true;
            }
            if (GetConnection().State == System.Data.ConnectionState.Closed)
            {
                return true;
            }

            return false;
        }

    }
}
