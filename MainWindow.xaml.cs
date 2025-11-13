using PCShop.Models;
using PCShop.View;
using PCShop.View.Admin; // Thêm namespace cho View Admin
using PCShop.View.Staff;
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
            // ...
            if (_currentUser.Role == 1) 
            {
                
                AdminTitle.Visibility = Visibility.Collapsed;
                AdminSeparator.Visibility = Visibility.Collapsed;
                btnApproval.Visibility = Visibility.Collapsed;
                btnUserManagement.Visibility = Visibility.Collapsed;
                btnCategoryManagement.Visibility = Visibility.Collapsed;
                btnSupplierManagement.Visibility = Visibility.Collapsed;
                btnReport.Visibility = Visibility.Collapsed;

                
            }
            else 
            {
                
                OperationsTitle.Visibility = Visibility.Collapsed;
                btnProductManagement.Visibility = Visibility.Collapsed; // (Admin không quản lý sản phẩm?)
                btnStockEntry.Visibility = Visibility.Collapsed;
                btnStockExport.Visibility = Visibility.Collapsed;
            }
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
                    MainContent.Content = new DashboardView();
                    break;
                case "btnProductManagement":
                    MainContent.Content = new ProductManagementView(); // Quản lí sản phẩm
                    break;
                case "btnStockEntry":
                    MainContent.Content = new StockEntryView(_currentUser);
                    break;
                case "btnStockExport": // <-- SỬA LẠI DÒNG NÀY
                    MainContent.Content = new StockExportView(_currentUser); // Tải View Xuất kho
                    break;
                

                // --- Admin Functions ---
                case "btnUserManagement":
                    MainContent.Content = new UserManagementView(); // Tải View Quản lý User
                    break;
                case "btnApproval": // <-- SỬA LẠI DÒNG NÀY
                    MainContent.Content = new ApprovalView(_currentUser); // Tải View Phê duyệt
                    break;
                case "btnCategoryManagement":
                    MainContent.Content = new CategoryManagementView(); // Quản lí category
                    break;
                case "btnSupplierManagement":
                    MainContent.Content = new SupplierManagementView(); //Quản lí nhà cung 
                    break;
                case "btnReport":
                    MainContent.Content = new ReportView();
                    break;
            }
        }
    }
}