using System;
using MySql.Data.MySqlClient;

namespace Player
{
    public class DBConnector
    {
        public string Server { get; set; }
        public string DatabaseName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        public MySqlConnection Connection { get; set; }

        private static DBConnector _instance = null;
        public static DBConnector Instance()
        {
            if (_instance == null)
                _instance = new DBConnector();
            return _instance;
        }

        public bool IsConnect()
        {
            if (Connection == null)
            {
                if (String.IsNullOrEmpty(DatabaseName))
                    return false;
                string connstring = string.Format("Server={0}; database={1}; UID={2}; password={3}", Server, DatabaseName, UserName, Password);
                Connection = new MySqlConnection(connstring);
                Connection.Open();
            }
            else
            {
                if (Connection.State != System.Data.ConnectionState.Open)
                {
                    Connection.Open();
                }
            }
            return true;
        }

        public bool InsertValues(string table, string path, string author, string title)
        {
            try
            {
                if (!IsConnect())
                    return false;

                MySqlCommand comm = Connection.CreateCommand();
                comm.CommandText = $"INSERT INTO {table} (path, author, title) VALUES (@path, @author, @title)";
                comm.Parameters.AddWithValue("@path", path);
                comm.Parameters.AddWithValue("@author", author);
                comm.Parameters.AddWithValue("@title", title);
                comm.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return false;
            }
            finally
            {
                if (Connection.State == System.Data.ConnectionState.Open)
                    Connection.Close();
            }
        }

        public void Close()
        {
            if (Connection != null && Connection.State == System.Data.ConnectionState.Open)
                Connection.Close();
        }
    }
}
