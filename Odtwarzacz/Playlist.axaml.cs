using System;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MySql.Data.MySqlClient;

namespace Player
{
    public partial class Playlist : Window
    {
        public ListBox playlista;

        public Playlist()
        {
            InitializeComponent();
            playlista = this.FindControl<ListBox>("muzyka"); // Find and initialize the ListBox from XAML
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public void Add(string item)
        {
            if (playlista != null)
            {
                playlista.Items.Add(item); // Add item to the ListBox
            }
        }
        public void LoadSongsFromDatabase()
        {
            DBConnector dbConnector = DBConnector.Instance();
            dbConnector.Server = "10.0.2.3";
            dbConnector.DatabaseName = "dwierzbicki";
            dbConnector.UserName = "dwierzbicki";
            dbConnector.Password = "Jui!#der7692@";

            if (dbConnector.IsConnect())
            {
                try
                {
                    string query = "SELECT title FROM songs";
                    MySqlCommand command = new MySqlCommand(query, dbConnector.Connection);
                    MySqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        string title = reader.GetString("title");
                        Add(title); // Add each title to the playlist
                    }

                    reader.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error loading songs from the database: " + ex.Message);
                }
                finally
                {
                    dbConnector.Close();
                }
            }
            else
            {
                Console.WriteLine("Failed to connect to the database.");
            }
        }

    }
}
