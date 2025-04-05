using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using RestSharp;
using Newtonsoft.Json;
using Project4_Client.Models;
using System.IO;
using Project4_Client.Config;
namespace Project4_Client.Pages
{
    /// <summary>
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class HomePage : Page
    {
        private MainWindow _mainWindow;
        private string _authToken;
        private string _currentUserId;
        private List<User> _allUsers;
        private int _currentUserIndex;

        public HomePage()
        {
            InitializeComponent();
            _mainWindow = Application.Current.MainWindow as MainWindow;
            InitializeData();
        }

        public HomePage(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            InitializeData();
        }

        private void InitializeData()
        {
            _authToken = App.Current.Properties["AuthToken"]?.ToString() ?? string.Empty;
            _currentUserId = App.Current.Properties["UserId"]?.ToString() ?? string.Empty;
            _currentUserIndex = 0;
            LoadUsers();
        }

        private async void LoadUsers()
        {
            try
            {
                var client = new RestClient(AppConfig.ServerBaseUrl);
                var request = new RestRequest("api/users", Method.Get);
                request.AddHeader("Authorization", $"Bearer {_authToken}");

                var response = await client.ExecuteAsync(request);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    _allUsers = JsonConvert.DeserializeObject<List<User>>(response.Content);
                    // Filter out the current user
                    _allUsers = _allUsers.Where(u => u.userId.ToString() != _currentUserId).ToList();
                    if (_allUsers.Any())
                    {
                        DisplayCurrentUser();
                    }
                    else
                    {
                        MessageBox.Show("No other users found.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    MessageBox.Show("Failed to load users.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DisplayCurrentUser()
        {
            if (_allUsers == null || !_allUsers.Any() || _currentUserIndex < 0 || _currentUserIndex >= _allUsers.Count)
                return;

            var user = _allUsers[_currentUserIndex];
            
            // Update UI elements
            NameTextBlock.Text = $"Name: {user.username}";
            AgeTextBlock.Text = $"Age: {user.age}";
            BioTextBlock.Text = $"Bio: {user.bio}";

            // Display profile image if available
            if (user.images != null && user.images.Length > 0 && !string.IsNullOrEmpty(user.images[0].imageData))
            {
                try
                {
                    byte[] imageBytes = Convert.FromBase64String(user.images[0].imageData);
                    using (MemoryStream ms = new MemoryStream(imageBytes))
                    {
                        var image = new BitmapImage();
                        image.BeginInit();
                        image.CacheOption = BitmapCacheOption.OnLoad;
                        image.StreamSource = ms;
                        image.EndInit();
                        ProfileImage.Source = image;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to load image: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                // Set default image if no image is available
                ProfileImage.Source = new BitmapImage(new Uri("/Assets/default-profile.png", UriKind.Relative));
            }
        }

        private async void LikeButton_Click(object sender, RoutedEventArgs e)
        {
            if (_allUsers == null || !_allUsers.Any() || _currentUserIndex < 0 || _currentUserIndex >= _allUsers.Count)
                return;

            var currentUser = _allUsers[_currentUserIndex];
            try
            {
                var client = new RestClient(AppConfig.ServerBaseUrl);
                var request = new RestRequest("api/likes", Method.Post);
                request.AddHeader("Authorization", $"Bearer {_authToken}");

                var likeDto = new LikeDto
                {
                    LikedId = currentUser.userId.ToString()
                };
                request.AddJsonBody(likeDto);

                var response = await client.ExecuteAsync(request);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var likeResponse = JsonConvert.DeserializeObject<LikeResponseDto>(response.Content);
                    
                    if (likeResponse.Like.likedBack)
                    {
                        MessageBox.Show("It's a match! 💖", "Match!", MessageBoxButton.OK, MessageBoxImage.Information);
                    }

                    // Remove the current user from the list
                    _allUsers.RemoveAt(_currentUserIndex);
                    
                    if (_allUsers.Any())
                    {
                        // If there are more users, show the next one
                        _currentUserIndex = _currentUserIndex % _allUsers.Count;
                        DisplayCurrentUser();
                    }
                    else
                    {
                        MessageBox.Show("No more users to show.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    MessageBox.Show("Failed to like user.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SkipButton_Click(object sender, RoutedEventArgs e)
        {
            if (_allUsers == null || !_allUsers.Any())
                return;

            // Remove the current user from the list
            _allUsers.RemoveAt(_currentUserIndex);
            
            if (_allUsers.Any())
            {
                // If there are more users, show the next one
                _currentUserIndex = _currentUserIndex % _allUsers.Count;
                DisplayCurrentUser();
            }
            else
            {
                MessageBox.Show("No more users to show.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            if (_allUsers == null || !_allUsers.Any())
                return;

            _currentUserIndex = (_currentUserIndex - 1 + _allUsers.Count) % _allUsers.Count;
            DisplayCurrentUser();
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (_allUsers == null || !_allUsers.Any())
                return;

            _currentUserIndex = (_currentUserIndex + 1) % _allUsers.Count;
            DisplayCurrentUser();
        }

        private void AccountButton_Click(object sender, RoutedEventArgs e)
        {
            if (_mainWindow != null)
            {
                _mainWindow.MainFrame.Navigate(new AccountInfo(_mainWindow));
            }
            else
            {
                MessageBox.Show("Navigation error: Cannot access main window.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
