using PCShop.Models;
using PCShop.Repository;
using System;
using System.Windows;
using System.Windows.Controls;

namespace PCShop.View.Admin
{
    public partial class ApprovalView : UserControl
    {
        private readonly StockEntryRepository _stockEntryRepo;
        private readonly SalesOrderRepository _salesOrderRepo;
        private readonly User _currentUser;

        private StockEntry _selectedImport;
        private SalesOrder _selectedExport;

        public ApprovalView(User currentUser)
        {
            InitializeComponent();
            _currentUser = currentUser;
            _stockEntryRepo = new StockEntryRepository();
            _salesOrderRepo = new SalesOrderRepository();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            LoadImportRequests();
            LoadExportRequests();
        }

        // --- LOGIC TAB NHẬP KHO ---
        private void LoadImportRequests()
        {
            try
            {
                dgImportRequests.ItemsSource = _stockEntryRepo.GetPendingEntries();

                // Reset trạng thái chi tiết
                gbImportDetails.IsEnabled = false;
                dgImportDetailsItems.ItemsSource = null;
                _selectedImport = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải phiếu nhập: " + ex.Message);
            }
        }

        private void dgImportRequests_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgImportRequests.SelectedItem is StockEntry entry)
            {
                _selectedImport = entry;
                gbImportDetails.IsEnabled = true; // Mở khóa khu vực chi tiết

                // Tải chi tiết sản phẩm của phiếu này
                dgImportDetailsItems.ItemsSource = _stockEntryRepo.GetDetailsForEntry(entry.EntryId);
            }
        }

        private void btnApproveImport_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedImport == null) return;

            if (MessageBox.Show("Xác nhận DUYỆT phiếu nhập này? Tồn kho sẽ tăng lên.", "Xác nhận Duyệt", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    _stockEntryRepo.ApproveStockEntry(_selectedImport.EntryId, _currentUser.UserId);
                    MessageBox.Show("Đã duyệt phiếu nhập thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Tải lại danh sách (phiếu này sẽ biến mất khỏi danh sách chờ)
                    LoadImportRequests();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi duyệt: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnRejectImport_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedImport == null) return;

            if (MessageBox.Show("Xác nhận TỪ CHỐI phiếu nhập này?", "Xác nhận Từ chối", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    _stockEntryRepo.RejectStockEntry(_selectedImport.EntryId, _currentUser.UserId);
                    MessageBox.Show("Đã từ chối phiếu nhập.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadImportRequests();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi từ chối: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // --- LOGIC TAB XUẤT KHO ---
        private void LoadExportRequests()
        {
            try
            {
                dgExportRequests.ItemsSource = _salesOrderRepo.GetPendingSalesOrders();

                // Reset trạng thái chi tiết
                gbExportDetails.IsEnabled = false;
                dgExportDetailsItems.ItemsSource = null;
                _selectedExport = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải phiếu xuất: " + ex.Message);
            }
        }

        private void dgExportRequests_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgExportRequests.SelectedItem is SalesOrder order)
            {
                _selectedExport = order;
                gbExportDetails.IsEnabled = true;

                // Tải chi tiết sản phẩm của phiếu xuất này
                dgExportDetailsItems.ItemsSource = _salesOrderRepo.GetDetailsForSalesOrder(order.SalesId);
            }
        }

        private void btnApproveExport_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedExport == null) return;

            if (MessageBox.Show("Xác nhận DUYỆT phiếu xuất này? Tồn kho sẽ bị trừ.", "Xác nhận Duyệt", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    _salesOrderRepo.ApproveSalesOrder(_selectedExport.SalesId, _currentUser.UserId);
                    MessageBox.Show("Đã duyệt phiếu xuất thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadExportRequests();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi duyệt: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnRejectExport_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedExport == null) return;

            if (MessageBox.Show("Xác nhận TỪ CHỐI phiếu xuất này?", "Xác nhận Từ chối", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    _salesOrderRepo.RejectSalesOrder(_selectedExport.SalesId, _currentUser.UserId);
                    MessageBox.Show("Đã từ chối phiếu xuất.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadExportRequests();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi từ chối: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}