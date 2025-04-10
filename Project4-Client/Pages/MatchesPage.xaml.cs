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
        private readonly JsonSerializerSettings _jsonSettings;

        public MatchesPage()
        {
            try
            {
                InitializeComponent();
                _authToken = App.Current.Properties["AuthToken"]?.ToString() ?? string.Empty;
                _currentUserId = App.Current.Properties["UserId"]?.ToString() ?? string.Empty;

                // Initialize JSON settings with case-insensitive property names
                _jsonSettings = new JsonSerializerSettings
                {
                    ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver
                    {
                        NamingStrategy = new Newtonsoft.Json.Serialization.CamelCaseNamingStrategy()
                    }
                };

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
                LoadMatches();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing page: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    var matchesResponse = JsonConvert.DeserializeObject<List<MatchDto>>(response.Content, _jsonSettings);
                    matches.Clear();

                    if (matchesResponse != null)
                    {
                        foreach (var match in matchesResponse)
                        {
                            var matchedUser = match.User1Id == _currentUserId ? match.User2 : match.User1;
                            if (matchedUser != null)
                            {
                                // Convert the first image to a BitmapImage
                                if (matchedUser.Images != null && matchedUser.Images.Count > 0)
                                {
                                    try
                                    {
                                        Console.WriteLine($"Loading image for user {matchedUser.Username}");
                                        Console.WriteLine($"Image data length: {matchedUser.Images[0].ImageData?.Length ?? 0}");
                                        var imageBytes = Convert.FromBase64String(matchedUser.Images[0].ImageData);
                                        Console.WriteLine($"Converted to {imageBytes.Length} bytes");
                                        using (var ms = new MemoryStream(imageBytes))
                                        {
                                            var bitmap = new BitmapImage();
                                            bitmap.BeginInit();
                                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                                            bitmap.StreamSource = ms;
                                            bitmap.EndInit();
                                            matchedUser.ProfileImage = bitmap;
                                            Console.WriteLine($"Successfully created BitmapImage for {matchedUser.Username}");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"Error loading image for {matchedUser.Username}: {ex.Message}");
                                        Console.WriteLine($"Stack trace: {ex.StackTrace}");
                                        matchedUser.ProfileImage = new BitmapImage(new Uri("pack://application:,,,/Project4-Client;component/Resources/default-image.png", UriKind.Absolute));
                                    }
                                }
                                else
                                {
                                    matchedUser.ProfileImage = new BitmapImage(new Uri("pack://application:,,,/Project4-Client;component/Resources/default-image.png", UriKind.Absolute));
                                }
                                matches.Add(matchedUser);
                            }
                        }

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
            MatchNameText.Text = user.Username;
            MatchEmailText.Text = user.Email;
            MatchAgeText.Text = $"Age: {user.Age}";
            MatchBioText.Text = user.Bio;

            if (user.ProfileImage != null)
            {
                MatchProfileImage.Source = user.ProfileImage;
            }
            else if (user.Images != null && user.Images.Count > 0)
            {
                try
                {
                    var imageBytes = Convert.FromBase64String(user.Images[0].ImageData);
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
                    MatchProfileImage.Source = new BitmapImage(new Uri("pack://application:,,,/Project4-Client;component/Resources/default-image.png", UriKind.Absolute));
                }
            }
            else
            {
                MatchProfileImage.Source = new BitmapImage(new Uri("pack://application:,,,/Project4-Client;component/Resources/default-image.png", UriKind.Absolute));
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }
    }
} 