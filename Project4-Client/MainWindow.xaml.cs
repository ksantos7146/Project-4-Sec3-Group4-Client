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
using ClassLib1;

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
        }

        // global variable
        EmpConnection.Rootobject result; // store the result of the API Conversion




        private void Button_Click(object sender, RoutedEventArgs e)
        {
            getData();
        }

        private void getData()
        {
            //Calls the API
            //Needs 'using RestSharp' <-- see line 1!
            //Source --> https://dummy.restapiexample.com/
            //API to be used --> https://dummy.restapiexample.com/api/v1/employees
            //Note the difference, I have chopped off the final bit of the address!
            var client = new RestClient("https://dummy.restapiexample.com/api/v1/");

            var request = new RestRequest("employees");  //The last bit of the api address
            var response = client.Execute(request);  //Request is ready

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                string rawResponse = response.Content;  //Raw data (needs refinement!)

                //Refinement ideally requires "PostMan" as this will show you what the 
                //data will look like after parsing and then allow you to use
                //Edit/Paste as JSON classes, I simply copied the structure into the 
                //Classes below

                //Convert the raw data (next line requires 'using Newton.Json'
                result = JsonConvert.DeserializeObject<EmpConnection.Rootobject>(rawResponse);

                if (result != null) // if we have some data
                {
                    foreach (var obj in result.data) 
                    {
                        listBox1.Items.Add(obj.employee_name); // put the employee name into the listbox
                    }
                }

            }

        }

        private void listBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            getData();
        }
    }
}