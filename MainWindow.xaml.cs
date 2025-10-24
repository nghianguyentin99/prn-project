using PCShop.Models;
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

namespace PCShop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private User _currentUser;

        public MainWindow(User user)
        {
            InitializeComponent();
            
            _currentUser = user;

            Title = $"PCShop - Xin chào, {_currentUser.FullName}";

       

            //if (_currentUser.Role == 1) // Staff
            //{
            //    // Ẩn hoặc khóa chức năng admin
            //    btnUserManagement.IsEnabled = false;
            //}
        }
    }
}