using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

// added dependencies and reference to the class
using RestSharp;
using Newtonsoft.Json;

using Project4_Client.Pages;

namespace Project4_Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
            MainFrame.Navigate(new LoginPage(this)); // Pass reference for navigation
        }



        //public MainWindow()
        //{
        //    InitializeComponent();
        //}

        //// global variable
        //List<User> allUsers; // store the result of the API Conversion




        //private void Button_Click(object sender, RoutedEventArgs e)
        //{
        //    getData();
        //}

        //private void getData()
        //{

        //    //var client = new RestClient("https://retoolapi.dev/zpuRkh/");
        //    var client = new RestClient("http://10.144.122.176:5214");

        //    var request = new RestRequest("/api/users");  // the last bit of the api address
        //    var response = client.Execute(request);  // request is ready

        //    if (response.StatusCode == System.Net.HttpStatusCode.OK)
        //    {
        //        string rawResponse = response.Content;  //Raw data (needs refinement!)


        //        //Convert the raw data (next line requires 'using Newton.Json'
        //        allUsers = JsonConvert.DeserializeObject<List<User>>(rawResponse);
        //    }

        //}

        //private void listBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    getData();
        //}
    }
}