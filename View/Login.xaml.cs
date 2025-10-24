using PCShop.Repository;
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
using System.Windows.Shapes;

namespace PCShop.View
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        private readonly UserRepository _userRepo;

        public Login()
        {
            InitializeComponent();
            _userRepo = new UserRepository();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Password.Trim();

            var user = _userRepo.Login(username, password);

            if (user != null)
            {
                MessageBox.Show($"Chào {user.FullName} ({(user.Role == 0 ? "Admin" : "Nhân viên")})!",
                    "Đăng nhập thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                // Mở MainWindow
                MainWindow main = new MainWindow(user);
                main.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Sai tên đăng nhập hoặc mật khẩu!",
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

