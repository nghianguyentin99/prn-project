using PCShop.Repository;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace PCShop.View.Admin
{
    public partial class ReportView : UserControl
    {
        private readonly ReportRepository _reportRepo;

        public ReportView()
        {
            InitializeComponent();
            _reportRepo = new ReportRepository();

            // Mặc định chọn từ ngày 1 tháng này đến hôm nay
            var today = DateTime.Now;
            var firstDay = new DateTime(today.Year, today.Month, 1);

            // Set ngày cho tab Kho
            dpStockStart.SelectedDate = firstDay;
            dpStockEnd.SelectedDate = today;

            // Set ngày cho tab Tài chính
            dpFinanceStart.SelectedDate = firstDay;
            dpFinanceEnd.SelectedDate = today;
        }

        // --- XỬ LÝ TAB 1: LỊCH SỬ KHO ---
        private void btnViewStock_Click(object sender, RoutedEventArgs e)
        {
            if (dpStockStart.SelectedDate == null || dpStockEnd.SelectedDate == null)
            {
                MessageBox.Show("Vui lòng chọn khoảng thời gian.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime start = dpStockStart.SelectedDate.Value.Date; // 00:00:00
            DateTime end = dpStockEnd.SelectedDate.Value.Date.AddDays(1).AddSeconds(-1); // 23:59:59
            string type = ((ComboBoxItem)cmbMoveType.SelectedItem).Tag.ToString();

            try
            {
                var data = _reportRepo.GetStockMovements(start, end, type);
                dgStockHistory.ItemsSource = data;

                if (data.Count == 0)
                {
                    MessageBox.Show("Không tìm thấy dữ liệu kho trong khoảng thời gian này.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải dữ liệu kho: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // --- XỬ LÝ TAB 2: TÀI CHÍNH ---
        private void btnViewFinance_Click(object sender, RoutedEventArgs e)
        {
            if (dpFinanceStart.SelectedDate == null || dpFinanceEnd.SelectedDate == null)
            {
                MessageBox.Show("Vui lòng chọn khoảng thời gian.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime start = dpFinanceStart.SelectedDate.Value.Date;
            DateTime end = dpFinanceEnd.SelectedDate.Value.Date.AddDays(1).AddSeconds(-1);

            try
            {
                // 1. Lấy dữ liệu Bán hàng (Revenue)
                var sales = _reportRepo.GetSalesRevenue(start, end);
                decimal totalRevenue = sales.Sum(s => s.TotalAmount ?? 0);

                // 2. Lấy dữ liệu Nhập hàng (Expenditure)
                var imports = _reportRepo.GetImportExpenditure(start, end);
                decimal totalExpenditure = imports.Sum(i => i.TotalAmount ?? 0);

                // 3. Tính dòng tiền
                decimal netCashFlow = totalRevenue - totalExpenditure;

                // 4. Hiển thị lên giao diện
                txtTotalRevenue.Text = totalRevenue.ToString("N0") + " ₫";
                txtTotalExpenditure.Text = totalExpenditure.ToString("N0") + " ₫";
                txtNetCashFlow.Text = netCashFlow.ToString("N0") + " ₫";

                // Đổi màu dòng tiền (Lời: Xanh, Lỗ: Đỏ)
                if (netCashFlow >= 0)
                    txtNetCashFlow.Foreground = System.Windows.Media.Brushes.SeaGreen;
                else
                    txtNetCashFlow.Foreground = System.Windows.Media.Brushes.Crimson;

                // 5. Đổ dữ liệu vào 2 DataGrid chi tiết
                dgSalesDetails.ItemsSource = sales;
                dgImportDetails.ItemsSource = imports;

                if (sales.Count == 0 && imports.Count == 0)
                {
                    MessageBox.Show("Không có dữ liệu tài chính nào trong khoảng thời gian này.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải dữ liệu tài chính: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}