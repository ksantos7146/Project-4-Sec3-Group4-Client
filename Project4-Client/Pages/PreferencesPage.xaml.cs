using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RestSharp;
using Newtonsoft.Json;
using Project4_Client.Models;
using Project4_Client.Config;

namespace Project4_Client.Pages
{
    public partial class PreferencesPage : Page
    {
        private const int MIN_AGE = 18;
        private const int MAX_AGE = 99;
        private readonly Regex _numericRegex = new Regex("[^0-9]+");
        private string _authToken;
        private string _currentUserId;

        public PreferencesPage()
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

                // Initialize with default values
                if (MinAgeTextBox != null) MinAgeTextBox.Text = MIN_AGE.ToString();
                if (MaxAgeTextBox != null) MaxAgeTextBox.Text = MAX_AGE.ToString();
                if (GenderPreferenceComboBox != null) GenderPreferenceComboBox.SelectedIndex = 0; // Select "Everyone" by default

                LoadCurrentPreferences();
                
                // Only validate ages if all UI elements are initialized
                if (MinAgeTextBox != null && MaxAgeTextBox != null && 
                    MinAgeError != null && MaxAgeError != null && 
                    SaveButton != null)
                {
                    ValidateAges(); // Initial validation
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing preferences: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                NavigationService?.GoBack();
            }
        }

        private async void LoadCurrentPreferences()
        {
            try
            {
                var client = new RestClient(AppConfig.ServerBaseUrl);
                var request = new RestRequest($"api/preference/user/{_currentUserId}", Method.Get);
                request.AddHeader("Authorization", $"Bearer {_authToken}");

                var response = await client.ExecuteAsync(request);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var preference = JsonConvert.DeserializeObject<PreferenceDto>(response.Content);
                    if (preference != null)
                    {
                        // Set the UI values
                        MinAgeTextBox.Text = preference.MinAge?.ToString() ?? MIN_AGE.ToString();
                        MaxAgeTextBox.Text = preference.MaxAge?.ToString() ?? MAX_AGE.ToString();
                        
                        // Set gender preference
                        if (preference.GenderId.HasValue)
                        {
                            int genderId = preference.GenderId.Value;
                            GenderPreferenceComboBox.SelectedIndex = genderId - 1 >= 0 && genderId - 1 < GenderPreferenceComboBox.Items.Count 
                                ? genderId - 1 
                                : 0; // Default to "Everyone" if invalid
                        }
                        else
                        {
                            GenderPreferenceComboBox.SelectedIndex = 0; // Default to "Everyone"
                        }
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    // No preferences found, keep defaults
                    MinAgeTextBox.Text = MIN_AGE.ToString();
                    MaxAgeTextBox.Text = MAX_AGE.ToString();
                    GenderPreferenceComboBox.SelectedIndex = 0;
                }
                else
                {
                    MessageBox.Show($"Failed to load preferences. Status: {response.StatusCode}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while loading preferences: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            e.Handled = _numericRegex.IsMatch(e.Text);
        }

        private void AgeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateAges();
        }

        private bool ValidateAges()
        {
            if (MinAgeTextBox == null || MaxAgeTextBox == null || 
                MinAgeError == null || MaxAgeError == null || 
                SaveButton == null)
            {
                return false;
            }

            bool isValid = true;
            SaveButton.IsEnabled = true;

            // Initialize with default values
            int minAge = MIN_AGE;
            int maxAge = MAX_AGE;

            // Validate Min Age
            if (string.IsNullOrEmpty(MinAgeTextBox.Text) || 
                !int.TryParse(MinAgeTextBox.Text, out minAge) || 
                minAge < MIN_AGE || 
                minAge > MAX_AGE)
            {
                MinAgeError.Text = $"Age must be between {MIN_AGE} and {MAX_AGE}";
                MinAgeError.Visibility = Visibility.Visible;
                isValid = false;
            }
            else
            {
                MinAgeError.Visibility = Visibility.Collapsed;
            }

            // Validate Max Age
            if (string.IsNullOrEmpty(MaxAgeTextBox.Text) || 
                !int.TryParse(MaxAgeTextBox.Text, out maxAge) || 
                maxAge < MIN_AGE || 
                maxAge > MAX_AGE)
            {
                MaxAgeError.Text = $"Age must be between {MIN_AGE} and {MAX_AGE}";
                MaxAgeError.Visibility = Visibility.Visible;
                isValid = false;
            }
            else
            {
                MaxAgeError.Visibility = Visibility.Collapsed;
            }

            // Validate Min Age <= Max Age
            if (isValid && minAge > maxAge)
            {
                MaxAgeError.Text = "Maximum age must be greater than or equal to minimum age";
                MaxAgeError.Visibility = Visibility.Visible;
                isValid = false;
            }

            SaveButton.IsEnabled = isValid;
            return isValid;
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateAges())
            {
                MessageBox.Show("Please correct the age range values before saving.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var client = new RestClient(AppConfig.ServerBaseUrl);
                var request = new RestRequest($"api/preference/user/{_currentUserId}", Method.Put);
                request.AddHeader("Authorization", $"Bearer {_authToken}");

                var preferenceDto = new PreferenceDto
                {
                    UserId = _currentUserId,
                    MinAge = int.Parse(MinAgeTextBox.Text),
                    MaxAge = int.Parse(MaxAgeTextBox.Text),
                    GenderId = GetGenderPreferenceId()
                };

                request.AddJsonBody(preferenceDto);
                var response = await client.ExecuteAsync(request);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    MessageBox.Show("Preferences saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    NavigationService?.GoBack();
                }
                else
                {
                    string errorMessage = "Failed to save preferences.";
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
                        // If we can't parse the error message, use the status code
                        errorMessage = $"Failed to save preferences. Status: {response.StatusCode}";
                    }
                    MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}\n\nPlease try again later.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private int? GetGenderPreferenceId()
        {
            if (GenderPreferenceComboBox.SelectedItem == null) return null;
            return (GenderPreferenceComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() switch
            {
                "Everyone" => null,
                "Men" => 1,
                "Women" => 2,
                _ => null
            };
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }
    }
} 