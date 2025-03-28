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

        private void loginRequest(string username, string password)
        {

            var client = new RestClient("http://10.144.116.108:5214/");

            var request = new RestRequest("api/Auth/login");  // the last bit of the api address
            var response = client.Execute(request);  // request is ready

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                string rawResponse = response.Content;  //Raw data (needs refinement!)



            }
            else
            {
                MessageBox.Show(response.Content ?? "Unknown error occurred.", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
    }



    // global variable
    //List<User> allUsers; // store the result of the API Conversion

}
