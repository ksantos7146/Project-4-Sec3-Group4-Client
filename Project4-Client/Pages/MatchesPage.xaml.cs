using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Project4_Client.Models;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Media;
using RestSharp;
using Newtonsoft.Json;
using Project4_Client.Config;

namespace Project4_Client.Pages
{
    public partial class MatchesPage : Page
    {
        private List<User> matches;
        private string _authToken;
        private string _currentUserId;

        public MatchesPage()
        {
            try
            {
                InitializeComponent();
                _authToken = App.Current.Properties["AuthToken"]?.ToString() ?? string.Empty;
                _currentUserId = App.Current.Properties["UserId"]?.ToString() ?? string.Empty;

                if (string.IsNullOrEmpty(_authToken) || string.IsNullOrEmpty(_currentUserId))
                {
                    MessageBox.Show("Authentication error. Please log in again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    var mainWindow = Application.Current.MainWindow as MainWindow;
                    if (mainWindow != null)
                    {
                        NavigationService?.Navigate(new LoginPage(mainWindow));
                    }
                    else
                    {
                        MessageBox.Show("Application error: Cannot access main window.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        NavigationService?.GoBack();
                    }
                    return;
                }

                matches = new List<User>();
                MatchesListView.ItemsSource = matches;
                LoadMatches();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing matches: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                NavigationService?.GoBack();
            }
        }

        private async void LoadMatches()
        {
            try
            {
                var client = new RestClient(AppConfig.ServerBaseUrl);
                var request = new RestRequest($"api/matches/user/{_currentUserId}", Method.Get);
                request.AddHeader("Authorization", $"Bearer {_authToken}");

                var response = await client.ExecuteAsync(request);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var matchDtos = JsonConvert.DeserializeObject<List<MatchDto>>(response.Content);
                    if (matchDtos != null)
                    {
                        matches.Clear();

                        // Get user details for each match
                        foreach (var match in matchDtos)
                        {
                            string matchedUserId = match.User1Id == _currentUserId ? match.User2Id : match.User1Id;
                            
                            var userRequest = new RestRequest($"api/users/{matchedUserId}", Method.Get);
                            userRequest.AddHeader("Authorization", $"Bearer {_authToken}");
                            
                            var userResponse = await client.ExecuteAsync(userRequest);
                            
                            if (userResponse.StatusCode == System.Net.HttpStatusCode.OK)
                            {
                                var user = JsonConvert.DeserializeObject<User>(userResponse.Content);
                                if (user != null)
                                {
                                    matches.Add(user);
                                }
                            }
                        }

                        MatchesListView.ItemsSource = null;
                        MatchesListView.ItemsSource = matches;

                        if (matches.Count == 0)
                        {
                            MessageBox.Show("No matches found.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    matches.Clear();
                    MatchesListView.ItemsSource = matches;
                    MessageBox.Show("No matches found.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show($"Failed to load matches. Status: {response.StatusCode}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading matches: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MatchesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MatchesListView.SelectedItem is User selectedUser)
            {
                UpdateMatchDetails(selectedUser);
            }
        }

        private void UpdateMatchDetails(User user)
        {
            MatchNameText.Text = user.username;
            MatchAgeText.Text = $"Age: {user.age}";
            MatchBioText.Text = user.bio;

            if (user.images != null && user.images.Length > 0)
            {
                try
                {
                    var imageBytes = Convert.FromBase64String(user.images[0].imageData);
                    using (var ms = new MemoryStream(imageBytes))
                    {
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.StreamSource = ms;
                        bitmap.EndInit();
                        MatchProfileImage.Source = bitmap;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading image: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }
    }
} 