using PCShop.Models;
using PCShop.View;
using PCShop.View.Admin; // Thêm namespace cho View Admin
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

            // Cập nhật Header với thông tin user (Cần sửa Header.xaml.cs)
            // MainHeader.SetUser(_currentUser); 

            // Thiết lập hiển thị dựa trên Role
            SetupRoleBasedAccess();

            // Tải View mặc định (Trang tổng quan)
            // MainContent.Content = new DashboardView(); // Sẽ tạo view này sau
            MainHeader.SetUser(_currentUser);

            MainContent.Content = new TextBlock { Text = "Chào mừng đến với PCShop!", FontSize = 24, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
        }

        private void SetupRoleBasedAccess()
        {
            // Cập nhật Title
            Title = $"PCShop - Xin chào, {_currentUser.FullName} ({(_currentUser.Role == 0 ? "Admin" : "Staff")})";

            // Role 1 là Staff
            if (_currentUser.Role == 1)
            {
                // Ẩn các nút của Admin
                btnUserManagement.Visibility = Visibility.Collapsed;
                btnCategoryManagement.Visibility = Visibility.Collapsed;
                btnReport.Visibility = Visibility.Collapsed;
                btnSupplierManagement.Visibility = Visibility.Collapsed;
                // Ẩn luôn cả dải phân cách và tiêu đề Admin
                AdminSeparator.Visibility = Visibility.Collapsed;
                AdminTitle.Visibility = Visibility.Collapsed;

                // (Các nghiệp vụ hạn chế khác của Staff sẽ được xử lý bên trong từng View)
            }
            // Role 0 (Admin) sẽ thấy tất cả (mặc định)
        }

        private void NavButton_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;
            if (clickedButton == null) return;

            // Xóa nền của tất cả các nút (nếu có style active)
            // (Tạm thời bỏ qua để giữ đơn giản)

            // Chuyển nội dung của ContentControl dựa trên nút được bấm
            switch (clickedButton.Name)
            {
                case "btnDashboard":
                    MainContent.Content = new TextBlock { Text = "Trang Tổng quan (Dashboard)", FontSize = 20, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                    break;
                case "btnProductManagement":
                    MainContent.Content = new ProductManagementView(); // Quản lí sản phẩm
                    break;
                case "btnStockEntry":
                    MainContent.Content = new TextBlock { Text = "Chức năng Quản lý Nhập kho", FontSize = 20, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                    break;
                case "btnStockExport":
                    MainContent.Content = new TextBlock { Text = "Chức năng Quản lý Xuất kho", FontSize = 20, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                    break;
                case "btnInventory":
                    MainContent.Content = new TextBlock { Text = "Chức năng Quản lý Tồn kho", FontSize = 20, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                    break;

                // --- Admin Functions ---
                case "btnUserManagement":
                    MainContent.Content = new UserManagementView(); // Tải View Quản lý User
                    break;
                case "btnCategoryManagement":
                    MainContent.Content = new CategoryManagementView(); // Quản lí category
                    break;
                case "btnSupplierManagement":
                    MainContent.Content = new SupplierManagementView(); //Quản lí nhà cung 
                    break;
                case "btnReport":
                    MainContent.Content = new TextBlock { Text = "Chức năng Báo cáo - Thống kê", FontSize = 20, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                    break;
            }
        }
    }
}