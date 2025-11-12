using PCShop.Models;
using PCShop.Repository;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace PCShop.View.Admin
{
    public partial class ApprovalView : UserControl
    {
        private readonly StockEntryRepository _stockEntryRepo;
        private readonly User _currentUser;
        private StockEntry _selectedEntry; // Lưu phiếu đang được chọn

        public ApprovalView(User currentUser)
        {
            InitializeComponent();
            _currentUser = currentUser;
            _stockEntryRepo = new StockEntryRepository();
        }

        // Tải danh sách các phiếu "Đang chờ" (Status = 0)
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            LoadPendingRequests();
        }

        private void LoadPendingRequests()
        {
            try
            {
                // Dùng hàm GetPendingEntries() chúng ta đã tạo
                dgPendingRequests.ItemsSource = _stockEntryRepo.GetPendingEntries();

                // Reset cột chi tiết
                gbRequestDetails.IsEnabled = false;
                dgRequestDetails.ItemsSource = null;
                txtDetailEntryId.Text = "";
                txtDetailNote.Text = "";
                _selectedEntry = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải danh sách chờ: {ex.Message}");
            }
        }

        // Khi Admin nhấn vào 1 phiếu trong danh sách chờ
        private void dgPendingRequests_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgPendingRequests.SelectedItem is StockEntry selectedEntry)
            {
                _selectedEntry = selectedEntry; // Lưu lại
                gbRequestDetails.IsEnabled = true; // Kích hoạt cột chi tiết

                // Hiển thị thông tin chung
                txtDetailEntryId.Text = selectedEntry.EntryId.ToString();
                txtDetailNote.Text = selectedEntry.Note;

                // Tải danh sách sản phẩm của phiếu đó
                try
                {
                    // Dùng hàm GetDetailsForEntry() mới thêm
                    dgRequestDetails.ItemsSource = _stockEntryRepo.GetDetailsForEntry(selectedEntry.EntryId);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi tải chi tiết phiếu: {ex.Message}");
                }
            }
        }

        // Nút "Phê duyệt"
        private void btnApprove_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedEntry == null) return;

            if (MessageBox.Show($"Bạn có chắc chắn muốn PHÊ DUYỆT phiếu nhập số {_selectedEntry.EntryId}?",
                                "Xác nhận Phê duyệt", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                return;
            }

            try
            {
                // Gọi hàm nghiệp vụ CHÍNH (tăng tồn kho)
                _stockEntryRepo.ApproveStockEntry(_selectedEntry.EntryId, _currentUser.UserId);
                MessageBox.Show("Phê duyệt thành công! Tồn kho đã được cập nhật.", "Thành công");

                // Tải lại danh sách (phiếu này sẽ biến mất)
                LoadPendingRequests();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi phê duyệt: {ex.Message}");
            }
        }

        // Nút "Từ chối"
        private void btnReject_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedEntry == null) return;

            if (MessageBox.Show($"Bạn có chắc chắn muốn TỪ CHỐI phiếu nhập số {_selectedEntry.EntryId}?",
                                "Xác nhận Từ chối", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
            {
                return;
            }

            try
            {
                // Gọi hàm nghiệp vụ "Từ chối" (chỉ set Status = 2)
                _stockEntryRepo.RejectStockEntry(_selectedEntry.EntryId, _currentUser.UserId);
                MessageBox.Show("Đã từ chối phiếu nhập.", "Thông báo");

                // Tải lại danh sách (phiếu này sẽ biến mất)
                LoadPendingRequests();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi từ chối: {ex.Message}");
            }
        }
    }
}