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
    /// Interaction logic for LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Page
    {
        private MainWindow _mainWindow;
        public LoginPage(MainWindow mw)
        {
            InitializeComponent();
            _mainWindow = mw;
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;
            loginRequest(username, password);
        }

        private void GoToRegister_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.MainFrame.Navigate(new RegisterPage(_mainWindow));
        }

        private async void loginRequest(string username, string password)
        {
            try
            {
                var client = new RestClient("http://10.144.116.108:5214/");
                var request = new RestRequest("api/Auth/login", Method.Post);
                
                // Add the login request body
                var loginRequest = new
                {
                    Username = username,
                    Password = password
                };
                request.AddJsonBody(loginRequest);

                var response = await client.ExecuteAsync(request);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var authResponse = JsonConvert.DeserializeObject<AuthResponse>(response.Content);
                    if (authResponse.Success)
                    {
                        // Store the token and user info
                        App.Current.Properties["AuthToken"] = authResponse.Token;
                        App.Current.Properties["UserId"] = authResponse.UserId;
                        App.Current.Properties["Username"] = authResponse.Username;

                        // Navigate to home page
                        _mainWindow.MainFrame.Navigate(new HomePage());
                    }
                    else
                    {
                        MessageBox.Show(authResponse.Message ?? "Login failed", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    var errorResponse = JsonConvert.DeserializeObject<AuthResponse>(response.Content);
                    MessageBox.Show(errorResponse?.Message ?? "Unknown error occurred.", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    // global variable
    //List<User> allUsers; // store the result of the API Conversion
}
