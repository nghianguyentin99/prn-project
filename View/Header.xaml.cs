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

namespace PCShop.View.Shared
{
    /// <summary>
    /// Interaction logic for Header.xaml
    /// </summary>
    public partial class Header : UserControl
    {
        public Header()
        {
            InitializeComponent();
        }

        // ---------- HÀM MỚI ----------
        // Hàm này được gọi bởi MainWindow để cập nhật thông tin
        public void SetUser(Models.User user)
        {
            if (user != null)
            {
                string role = (user.Role == 0) ? "Admin" : "Staff";
                txtUser.Text = $"Xin chào, {user.FullName} ({role})";
            }
        }
        // -----------------------------

        // ---------- SỬA ĐỔI LOGIC ĐĂNG XUẤT ----------
        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Bạn có chắc chắn muốn đăng xuất?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                // Tìm Window cha (MainWindow) và đóng nó
                Window parentWindow = Window.GetWindow(this);
                if (parentWindow != null)
                {
                    // Mở lại cửa sổ Login
                    Login loginWindow = new Login();
                    loginWindow.Show();

                    // Đóng cửa sổ hiện tại (MainWindow)
                    parentWindow.Close();
                }
            }
        }
        // ---------------------------------------------
    }
}