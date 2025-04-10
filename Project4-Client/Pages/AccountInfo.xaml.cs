﻿using System;
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
using Microsoft.Win32;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using Project4_Client.Config;
namespace Project4_Client.Pages
{
    /// <summary>
    /// Interaction logic for AccountInfo.xaml
    /// </summary>
    public partial class AccountInfo : Page
    {
        private MainWindow _mainWindow;
        private string _authToken;
        private string _userId;
        private string _currentImageData;

        public AccountInfo(MainWindow mw)
        {
            InitializeComponent();
            _mainWindow = mw;
            _authToken = App.Current.Properties["AuthToken"]?.ToString() ?? string.Empty;
            _userId = App.Current.Properties["UserId"]?.ToString() ?? string.Empty;
            LoadUserData();
        }

        private async void LoadUserData()
        {
            try
            {
                var client = new RestClient(AppConfig.ServerBaseUrl);
                var request = new RestRequest("api/users/logged", Method.Get);
                request.AddHeader("Authorization", $"Bearer {_authToken}");

                var response = await client.ExecuteAsync(request);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var user = JsonConvert.DeserializeObject<dynamic>(response.Content);
                    EmailTextBox.Text = user.email ?? string.Empty;
                    BioTextBox.Text = user.bio ?? string.Empty;
                    AgeTextBox.Text = user.age?.ToString() ?? string.Empty;
                    
                    // Set gender dropdown
                    if (user.genderId != null)
                    {
                        int genderId = (int)user.genderId;
                        GenderComboBox.SelectedIndex = genderId - 1 >= 0 && genderId - 1 < GenderComboBox.Items.Count 
                            ? genderId - 1 
                            : -1;
                    }
                    
                    // Load profile image if exists
                    if (user.images != null && user.images.Count > 0)
                    {
                        var imageData = user.images[0].imageData?.ToString();
                        if (!string.IsNullOrEmpty(imageData))
                        {
                            _currentImageData = imageData;
                            UpdateProfileImage(imageData);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Failed to load user data. Please try again later.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateProfileImage(string base64Image)
        {
            try
            {
                byte[] imageBytes = Convert.FromBase64String(base64Image);
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

        private async void UploadImageButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Filter = "Image files (*.jpg, *.jpeg, *.png)|*.jpg;*.jpeg;*.png",
                    Title = "Select a profile image"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    string filePath = openFileDialog.FileName;
                    byte[] imageBytes = File.ReadAllBytes(filePath);
                    string base64Image = Convert.ToBase64String(imageBytes);

                    // Update the image display
                    UpdateProfileImage(base64Image);
                    _currentImageData = base64Image;

                    // Upload the image to the server
                    var client = new RestClient(AppConfig.ServerBaseUrl);
                    var request = new RestRequest($"api/users/{_userId}/images", Method.Post);
                    request.AddHeader("Authorization", $"Bearer {_authToken}");

                    var imageDto = new
                    {
                        ImageData = base64Image,
                        UploadedAt = DateTime.UtcNow,
                        UserId = _userId
                    };

                    request.AddJsonBody(new[] { imageDto });
                    var response = await client.ExecuteAsync(request);

                    if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        MessageBox.Show("Failed to upload image. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.MainFrame.Navigate(new HomePage(_mainWindow));
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateInputs())
            {
                try
                {
                    var client = new RestClient(AppConfig.ServerBaseUrl);
                    var request = new RestRequest($"api/users/{_userId}", Method.Put);
                    request.AddHeader("Authorization", $"Bearer {_authToken}");

                    // Create update request body
                    var updateRequest = new
                    {
                        Email = EmailTextBox.Text,
                        Bio = BioTextBox.Text,
                        GenderId = GetGenderId(),
                        Age = int.Parse(AgeTextBox.Text),
                        Password = PasswordBox.SecurePassword.Length > 0 
                            ? new System.Net.NetworkCredential(string.Empty, PasswordBox.SecurePassword).Password 
                            : null
                    };

                    request.AddJsonBody(updateRequest);
                    var response = await client.ExecuteAsync(request);

                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        MessageBox.Show("Profile updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        _mainWindow.MainFrame.Navigate(new HomePage(_mainWindow));
                    }
                    else
                    {
                        var errorResponse = JsonConvert.DeserializeObject<dynamic>(response.Content);
                        MessageBox.Show(errorResponse?.message ?? "Failed to update profile. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(EmailTextBox.Text))
            {
                MessageBox.Show("Please enter an email", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Check if either password field has content
            bool hasPassword = PasswordBox.SecurePassword.Length > 0;
            bool hasConfirmPassword = ConfirmPasswordBox.SecurePassword.Length > 0;
            Console.WriteLine(PasswordBox.SecurePassword);
            Console.WriteLine(ConfirmPasswordBox.SecurePassword);
            // Only validate passwords if they are provided
            if (hasPassword || hasConfirmPassword)
            {
                if (hasPassword != hasConfirmPassword)
                {
                    MessageBox.Show("Please fill in both password fields", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                // Convert SecureString to string for comparison
                string password = new System.Net.NetworkCredential(string.Empty, PasswordBox.SecurePassword).Password;
                string confirmPassword = new System.Net.NetworkCredential(string.Empty, ConfirmPasswordBox.SecurePassword).Password;

                if (password != confirmPassword)
                {
                    MessageBox.Show("Passwords do not match", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
            }

            if (GenderComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a gender", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!int.TryParse(AgeTextBox.Text, out int age) || age < 18 || age > 100)
            {
                MessageBox.Show("Please enter a valid age (18-100)", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private int GetGenderId()
        {
            if (GenderComboBox.SelectedItem == null) return 0;
            return (GenderComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() switch
            {
                "Male" => 1,
                "Female" => 2,
                "Other" => 3,
                _ => 0
            };
        }

        private void GoToLogin_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.MainFrame.Navigate(new LoginPage(_mainWindow));
        }
    }
}
