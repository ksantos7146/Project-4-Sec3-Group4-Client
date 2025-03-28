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

namespace Project4_Client.Pages
{
    /// <summary>
    /// Interaction logic for RegisterPage.xaml
    /// </summary>
    public partial class RegisterPage : Page
    {
        private MainWindow _mainWindow;
        public RegisterPage(MainWindow mw)
        {
            InitializeComponent();
            _mainWindow = mw;
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateInputs())
            {
                var registerRequest = new RegisterRequest
                {
                    Username = UsernameTextBox.Text,
                    Email = EmailTextBox.Text,
                    Password = PasswordBox.Password,
                    Bio = BioTextBox.Text,
                    GenderId = GetGenderId(GenderComboBox.SelectedItem as ComboBoxItem),
                    StateId = GetStateId(StateComboBox.SelectedItem as ComboBoxItem),
                    Age = int.Parse(AgeTextBox.Text)
                };

                registerRequestFunc(registerRequest);
            }
        }

        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(UsernameTextBox.Text))
            {
                MessageBox.Show("Please enter a username", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(EmailTextBox.Text))
            {
                MessageBox.Show("Please enter an email", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                MessageBox.Show("Please enter a password", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (PasswordBox.Password != ConfirmPasswordBox.Password)
            {
                MessageBox.Show("Passwords do not match", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (GenderComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a gender", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (StateComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a state", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!int.TryParse(AgeTextBox.Text, out int age) || age < 18 || age > 100)
            {
                MessageBox.Show("Please enter a valid age (18-100)", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private int GetGenderId(ComboBoxItem selectedItem)
        {
            if (selectedItem == null) return 0;
            return selectedItem.Content.ToString() switch
            {
                "Male" => 1,
                "Female" => 2,
                "Other" => 3,
                _ => 0
            };
        }

        private int GetStateId(ComboBoxItem selectedItem)
        {
            if (selectedItem == null) return 0;
            return selectedItem.Content.ToString() switch
            {
                "Single" => 1,
                "In a Relationship" => 2,
                "Married" => 3,
                "Divorced" => 4,
                "Widowed" => 5,
                _ => 0
            };
        }

        private void GoToLogin_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.MainFrame.Navigate(new LoginPage(_mainWindow));
        }

        private async void registerRequestFunc(RegisterRequest request)
        {
            try
            {
                var client = new RestClient("http://10.144.116.108:5214/");
                var restRequest = new RestRequest("api/Auth/register", Method.Post);
                restRequest.AddJsonBody(request);

                var response = await client.ExecuteAsync(restRequest);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var authResponse = JsonConvert.DeserializeObject<AuthResponse>(response.Content);
                    if (authResponse.Success)
                    {
                        // Store the token and user info
                        App.Current.Properties["AuthToken"] = authResponse.Token;
                        App.Current.Properties["UserId"] = authResponse.UserId;
                        App.Current.Properties["Username"] = authResponse.Username;

                        MessageBox.Show("Registration successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        _mainWindow.MainFrame.Navigate(new HomePage());
                    }
                    else
                    {
                        MessageBox.Show(authResponse.Message ?? "Registration failed", "Registration Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    var errorResponse = JsonConvert.DeserializeObject<AuthResponse>(response.Content);
                    MessageBox.Show(errorResponse?.Message ?? "Unknown error occurred.", "Registration Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
