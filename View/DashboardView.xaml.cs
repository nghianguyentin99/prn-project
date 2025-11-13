using LiveCharts;
using LiveCharts.Wpf;
using PCShop.Repository;
using System;
using System.Collections.Generic; 
using System.Linq; 
using System.Windows;
using System.Windows.Controls;
namespace PCShop.View
{
    public partial class DashboardView : UserControl
    {
        private readonly DashboardRepository _dashboardRepo;

        public SeriesCollection SeriesCollection { get; set; }
        public string[] ProductLabels { get; set; }
        public Func<double, string> Formatter { get; set; }

        public DashboardView()
        {
            InitializeComponent();
            _dashboardRepo = new DashboardRepository();
            this.DataContext = this;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            LoadDashboardData();
            LoadChartData();
        }

        private void LoadDashboardData()
        {
            try
            {
                // 1. Lấy tổng số sản phẩm
                int totalProducts = _dashboardRepo.GetTotalProducts();
                txtTotalProducts.Text = totalProducts.ToString("N0");

                // 2. Lấy tổng giá trị tồn kho
                decimal totalValue = _dashboardRepo.GetTotalInventoryValue();
                txtTotalValue.Text = totalValue.ToString("N0") + " đ";

                // 3. Lấy số lượng sắp hết hàng
                int lowStock = _dashboardRepo.GetLowStockCount();
                txtLowStock.Text = lowStock.ToString("N0");

                // 4. Lấy số lượng chờ duyệt
                int pending = _dashboardRepo.GetPendingRequestsCount();
                txtPending.Text = pending.ToString("N0");

                txtTotalProducts.Text = _dashboardRepo.GetTotalProducts().ToString("N0");
                txtTotalValue.Text = _dashboardRepo.GetTotalInventoryValue().ToString("N0") + " đ";
                txtLowStock.Text = _dashboardRepo.GetLowStockCount().ToString("N0");
                txtPending.Text = _dashboardRepo.GetPendingRequestsCount().ToString("N0");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải dữ liệu Dashboard: {ex.Message}");
            }
        }

        private void LoadChartData()
        {
            try
            {
                // 1. Lấy dữ liệu từ Repository
                var topProducts = _dashboardRepo.GetTopSellingProducts();

                // 2. Chuẩn bị dữ liệu cho LiveCharts
                var values = new ChartValues<int>();
                var labels = new List<string>();

                foreach (var item in topProducts)
                {
                    labels.Add(item.Key);   // Tên sản phẩm
                    values.Add(item.Value); // Số lượng bán
                }

                // 3. Gán vào SeriesCollection (Biểu đồ cột)
                SeriesCollection = new SeriesCollection
                {
                    new ColumnSeries
                    {
                        Title = "Đã bán",
                        Values = values,
                        DataLabels = true, // Hiển thị số trên cột
                        Fill = System.Windows.Media.Brushes.CornflowerBlue // Màu cột
                    }
                };

                // 4. Gán Labels trục X
                ProductLabels = labels.ToArray();

                // 5. Cập nhật lại giao diện (vì mình set property sau khi InitializeComponent)
                // Thông thường WPF tự nhận nếu dùng PropertyChanged, 
                // nhưng ở đây gán trực tiếp thì cần gán lại DataContext hoặc setup từ Constructor.
                // Tuy nhiên, với LiveCharts, gán lại property thường tự update. 
                // Để chắc ăn, ta gán DataContext ở cuối:
                this.DataContext = null;
                this.DataContext = this;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải biểu đồ: {ex.Message}");
            }
        }
    }
}