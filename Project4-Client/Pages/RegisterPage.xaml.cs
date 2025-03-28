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
            // You can add validation here later
            _mainWindow.MainFrame.Navigate(new HomePage());
        }

        private void GoToLogin_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.MainFrame.Navigate(new LoginPage(_mainWindow));
        }
    }
}
