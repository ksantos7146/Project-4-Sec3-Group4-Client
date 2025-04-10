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
                    _allUsers = _allUsers.Where(u => u.UserId.ToString() != _currentUserId).ToList();
                    
                    // Get user preferences to filter by
                    var prefRequest = new RestRequest($"api/preference/user/{_currentUserId}", Method.Get);
                    prefRequest.AddHeader("Authorization", $"Bearer {_authToken}");
                    
                    var prefResponse = await client.ExecuteAsync(prefRequest);
                    
                    if (prefResponse.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var preferences = JsonConvert.DeserializeObject<PreferenceDto>(prefResponse.Content);
                        
                        // Apply age filter
                        if (preferences.MinAge.HasValue)
                        {
                            _allUsers = _allUsers.Where(u => u.Age >= preferences.MinAge.Value).ToList();
                        }
                        
                        if (preferences.MaxAge.HasValue)
                        {
                            _allUsers = _allUsers.Where(u => u.Age <= preferences.MaxAge.Value).ToList();
                        }
                        
                        // Apply gender filter if specified
                        if (preferences.GenderId.HasValue)
                        {
                            _allUsers = _allUsers.Where(u => {
                                // This assumes the user object has a genderId property
                                // You may need to adjust this based on your actual user model
                                return u.GetType().GetProperty("genderId")?.GetValue(u)?.ToString() == preferences.GenderId.Value.ToString();
                            }).ToList();
                        }
                    }
                    
                    if (_allUsers.Any())
                    {
                        DisplayCurrentUser();
                    }
                    else
                    {
                        MessageBox.Show("No users match your preferences.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
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
            NameTextBlock.Text = user.Username;
            if (user.Age >= 18 && user.Age <= 100)
            {
                AgeTextBlock.Text = $"Age: {user.Age}";
            }
            else
            {
                AgeTextBlock.Text = "Age: Not specified";
            }
            BioTextBlock.Text = user.Bio;

            // Display profile image if available
            if (user.Images != null && user.Images.Count > 0 && !string.IsNullOrEmpty(user.Images[0].ImageData))
            {
                try
                {
                    byte[] imageBytes = Convert.FromBase64String(user.Images[0].ImageData);
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
            {
                MessageBox.Show("No users available to like.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var currentUser = _allUsers[_currentUserIndex];
            try
            {
                var client = new RestClient(AppConfig.ServerBaseUrl);
                var request = new RestRequest("api/like", Method.Post);
                request.AddHeader("Authorization", $"Bearer {_authToken}");

                var likeDto = new LikeDto
                {
                    LikedId = currentUser.UserId.ToString(),
                    LikerId = _currentUserId,
                    LikedAt = DateTime.UtcNow,
                    LikedBack = false
                };
                request.AddJsonBody(likeDto);

                var response = await client.ExecuteAsync(request);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var likeResponse = JsonConvert.DeserializeObject<LikeResponseDto>(response.Content);
                    if (likeResponse != null && likeResponse.Like != null)
                    {
                        if (likeResponse.Like.LikedBack)
                        {
                            MessageBox.Show("It's a match! 💖", "Match!", MessageBoxButton.OK, MessageBoxImage.Information);
                        }

                        // Remove the current user from the list
                        _allUsers.RemoveAt(_currentUserIndex);
                        
                        // If the server provided a next user, add it to the list
                        if (likeResponse.NextUser != null)
                        {
                            // Check if the user is already in the list
                            if (!_allUsers.Any(u => u.UserId == likeResponse.NextUser.UserId))
                            {
                                _allUsers.Add(likeResponse.NextUser);
                            }
                        }
                        
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
                        MessageBox.Show("Invalid response from server.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    MessageBox.Show("Cannot like this user.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                {
                    // Try to get more detailed error information
                    string errorMessage = "Internal Server Error";
                    try
                    {
                        var errorResponse = JsonConvert.DeserializeObject<dynamic>(response.Content);
                        if (errorResponse != null && errorResponse.message != null)
                        {
                            errorMessage = errorResponse.message.ToString();
                        }
                    }
                    catch
                    {
                        // If we can't parse the error message, just use the default
                    }
                    
                    MessageBox.Show($"Server error: {errorMessage}\n\nPlease try again later.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    
                    // Log the error for debugging
                    Console.WriteLine($"Like error: {response.Content}");
                }
                else
                {
                    MessageBox.Show($"Failed to like user. Status: {response.StatusCode}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while liking user: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private void MatchesButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new MatchesPage());
        }

        private void PreferencesButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new PreferencesPage());
        }
    }
}
