using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Threading.Tasks;
using Avalonia.Controls.Primitives;
using System.Diagnostics;
using Avalonia.Threading;
using System.Timers;
using Avalonia.Media;
using MySql.Data.MySqlClient;

namespace Player
{
    public partial class MainWindow : Window
    {
        private NetCoreAudio.Player audio = new NetCoreAudio.Player();
        private bool isPlaying = false;
        string length;
        Timer timer = new Timer(1000);
        DateTime startTime;
        Playlist playlist = new Playlist();
        ListBoxItem item = new ListBoxItem();
        
        public MainWindow()
        {
            InitializeComponent();
            Slider1.Value = 0;
            Slider1.MaxWidth=300;
            timer.Elapsed+= Timer_Tick;
            playlist.Show();
            

        // Obrót slidera o 90 stopni
            Volume.RenderTransform = new RotateTransform(-90);
        }

        private void Back_OnClick(object? sender, RoutedEventArgs e)
        {
            
        }
        
        private string GetFilePathFromDatabase(string songTitle)
        {
            DBConnector dbConnector = DBConnector.Instance();
            dbConnector.Server = "10.0.2.3";
            dbConnector.DatabaseName = "dwierzbicki";
            dbConnector.UserName = "dwierzbicki";
            dbConnector.Password = "Jui!#der7692@";

            if (dbConnector.IsConnect())
            {
                string query = "SELECT path FROM songs WHERE title = @title";
                MySqlCommand command = new MySqlCommand(query, dbConnector.Connection);
                command.Parameters.AddWithValue("@title", songTitle);
                object result = command.ExecuteScalar();
                dbConnector.Close();

                if (result != null)
                {
                    return result.ToString();
                }
                else
                {
                    Console.WriteLine("File path not found for the selected song.");
                    return null;
                }
            }
            else
            {
                Console.WriteLine("Failed to connect to the database.");
                return null;
            }
        }
        private async void Play_OnClick(object? sender, RoutedEventArgs e)
        {
            if (audio.Playing)
            {
                audio.Stop().Wait();
                play.Content = "\u25b6";
                timer.Stop();
                Slider1.Value=0;
            }
            else
            {
                string selectedSongTitle = playlist.playlista.SelectedItem.ToString();
                mp3.Text = GetFilePathFromDatabase(selectedSongTitle);
                if (!string.IsNullOrEmpty(mp3.Text))
                {
                    await audio.Play(mp3.Text);
                    play.Content = "||"; // Change button content to pause symbol
                    timer.Start();
                }
                else
                {
                    Console.WriteLine("File path not found for the selected song.");
                }
                timer.Start();
                // Call the GetMp3Image method to retrieve image data not handled in user code: 'Object reference not set to an instance of an object.'
                
                //byte[] imageData = await GetMp3Image(mp3.Text);
                string title2 = await GetMp3Title(playlist.playlista.SelectedItem.ToString());
                string author2 = await GetMp3Author(playlist.playlista.SelectedItem.ToString());
                length = await GetMp3Length(playlist.playlista.SelectedItem.ToString());
                czas.Text = length;
                title.Text = title2;
                author.Text = author2;
                double length2 = await GetMp3Length2(mp3.Text);
                // Ustaw maksymalną wartość Slider1.Maximum na podstawie długości utworu
                Slider1.Maximum = Convert.ToDouble(length2);
                Slider1.Value=0;
                startTime = DateTime.Now;
                
                play.Content = "||";
            }

        }
        private void Volume_OnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
        {
            // Ustawienie głośności na podstawie wartości z Slider2
            audio.SetVolume(Convert.ToByte(Volume.Value)).Wait();
        }

        private void Next_OnClick(object? sender, RoutedEventArgs e)
        {
            
        }

        private async void Open_OnClick(object? sender, RoutedEventArgs e)
        {

            var dialog = new OpenFileDialog();

            dialog.Title = "Open file mp3";
            string[] result = await dialog.ShowAsync(this);
            string title2 = await GetMp3Title(result[0]);
            string author2 = await GetMp3Author(result[0]);
            length = await GetMp3Length(result[0]);
            mp3.Text = result[0];
            if (result != null)
            {
                try{
                    var db = new DBConnector();
                    db.Server = "10.0.2.3";
                    db.DatabaseName = "dwierzbicki";
                    db.UserName = "dwierzbicki";
                    db.Password = "Jui!#der7692@";
                    if(db.IsConnect()){
                        this.Title = "Połączono";
                        if(db.InsertValues("songs",result[0],author2,title2)){
                            this.Title = "Wykonano Polecenie";
                            playlist.LoadSongsFromDatabase();
                        }
                    }
                }
                catch(Exception ex){
                    Debug.WriteLine("Error: "+ ex.Message);
                }                
            }
            
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            // Call Dispatcher.UIThread.InvokeAsync to update the UI asynchronously
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                // Calculate the elapsed time since the playback started
                TimeSpan elapsedTime = DateTime.Now - startTime;

                // Update the Slider1.Value based on the elapsed time and total duration
                int currentPosition = Convert.ToInt32(elapsedTime.TotalSeconds);
                Slider1.Value = currentPosition;

                // If the playback completes, stop the timer
                if (currentPosition >= Slider1.Maximum)
                {
                    timer.Stop();
                }
                TimeSpan remainingTime = TimeSpan.FromSeconds(Slider1.Maximum - currentPosition);
                pozostalyCzas.Text = $"Pozostały czas {remainingTime:mm\\:ss}";
            });
        }
        private async Task<string> GetMp3Title(string filePath)
        {
            try
            {
                // Use the TagLib library to get metadata from the MP3 file
                var file = await Task.Run(() => TagLib.File.Create(filePath));

                // Check if the file has valid metadata
                if (file.Tag != null && !string.IsNullOrEmpty(file.Tag.Title))
                {
                    // Return the title if it exists
                    return file.Tag.Title;
                }
                else
                {
                    // Provide a default title if metadata is missing
                    return "Unknown Title";
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., if the file is not a valid MP3 file)
                Console.WriteLine($"Error reading MP3 file: {ex.Message}");
                return "Unknown Title";
            }
        }
        private async Task<byte[]> GetMp3Image(string filePath)
        {
            try
            {
                // Use the TagLib library to get metadata from the MP3 file
                var file = await Task.Run(() => TagLib.File.Create(filePath));

                // Check if the file has valid metadata and a cover image
                if (file.Tag != null && file.Tag.Pictures != null && file.Tag.Pictures.Length > 0)
                {
                    // Get the first cover image (assuming there might be multiple images)
                    var coverPicture = file.Tag.Pictures[0];

                    // Return the image data
                    return coverPicture.Data.Data;
                }
                else
                {
                    // Provide a default image or handle the case when no image is found
                    return null;
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., if the file is not a valid MP3 file)
                Console.WriteLine($"Error reading MP3 file: {ex.Message}");
                return null;
            }
        }

        private async Task<string> GetMp3Author(string filePath)
        {
            try
            {
                // Use the TagLib library to get metadata from the MP3 file
                var file = await Task.Run(() => TagLib.File.Create(filePath));

                // Check if the file has valid metadata
                if (file.Tag != null && !string.IsNullOrEmpty(file.Tag.FirstPerformer))
                {
                    // Return the title if it exists
                    return file.Tag.FirstPerformer;
                }
                else
                {
                    // Provide a default title if metadata is missing
                    return "Unknown Author";
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., if the file is not a valid MP3 file)
                Console.WriteLine($"Error reading MP3 file: {ex.Message}");
                return "Unknown Author";
            }
        }
        private async Task<string> GetMp3Length(string filePath)
        {
            try
            {
                // Use the TagLib library to get metadata from the MP3 file
                var file = await Task.Run(() => TagLib.File.Create(filePath));

                // Check if the file has valid metadata
                if (file.Properties != null)
                {
                    // Return the duration in a readable format (mm:ss)
                    return $"{file.Properties.Duration:mm\\:ss}";
                }
                else
                {
                    // Provide a default length if metadata is missing
                    return "Unknown Length";
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., if the file is not a valid MP3 file)
                Debug.WriteLine($"Error reading MP3 file: {ex.Message}");
                return "Unknown Length";
            }
        }
        private async Task<double> GetMp3Length2(string filePath)
        {
            try
            {
                // Use the TagLib library to get metadata from the MP3 file
                var file = await Task.Run(() => TagLib.File.Create(filePath));

                // Check if the file has valid metadata
                if (file.Properties != null)
                {
                    // Return the duration in a readable format (mm:ss)
                    return file.Properties.Duration.TotalSeconds;
                }
                else
                {
                    // Provide a default length if metadata is missing
                    return 1;
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., if the file is not a valid MP3 file)
                Debug.WriteLine($"Error reading MP3 file: {ex.Message}");
                return 1;
            }
        }

        private async void Slider1_OnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
        {
            // Użyj Dispatcher.UIThread.InvokeAsync do asynchronicznego aktualizowania interfejsu użytkownika
            
        }


    }
}