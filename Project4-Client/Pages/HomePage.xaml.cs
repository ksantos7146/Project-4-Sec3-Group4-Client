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
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class HomePage : Page
    {
        private MainWindow _mainWindow;

        public HomePage()
        {
            InitializeComponent();
            // Try to get the MainWindow reference from the current application
            _mainWindow = Application.Current.MainWindow as MainWindow;
        }

        public HomePage(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
        }

        private void AccountButton_Click(object sender, RoutedEventArgs e)
        {
            if (_mainWindow != null)
            {
                _mainWindow.MainFrame.Navigate(new AccountInfo(_mainWindow));
            }
            else
            {
                MessageBox.Show("Navigation error: Cannot access main window.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
