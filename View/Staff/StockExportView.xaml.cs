using PCShop.Models;
using PCShop.Repository;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using PCShop.View; // <== THÊM USING NÀY (để tái sử dụng ViewModel)

namespace PCShop.View.Staff
{
    // Chúng ta TÁI SỬ DỤNG ViewModel nội bộ của StockEntryView
    // vì nó 'public' và có cấu trúc y hệt
    using StockEntryDetailViewModel = PCShop.View.StockEntryView.StockEntryDetailViewModel;

    public partial class StockExportView : UserControl
    {
        // Repositories
        private readonly ProductRepository _productRepo;
        private readonly SalesOrderRepository _salesOrderRepo;
        private readonly User _currentUser;

        // Danh sách chi tiết
        private ObservableCollection<StockEntryDetailViewModel> _currentExportDetails;

        public StockExportView(User currentUser)
        {
            InitializeComponent();

            _productRepo = new ProductRepository();
            _salesOrderRepo = new SalesOrderRepository();
            _currentUser = currentUser;

            _currentExportDetails = new ObservableCollection<StockEntryDetailViewModel>();
            dgProductsToAdd.ItemsSource = _currentExportDetails;
        }

        // 1. Tải ComboBox sản phẩm
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Chỉ hiển thị các sản phẩm có tồn kho > 0
                cmbMasterProductList.ItemsSource = _productRepo.GetAll().Where(p => p.Quantity > 0).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải danh sách sản phẩm: {ex.Message}");
            }
        }

        // 2. Khi chọn sản phẩm -> Tự động điền giá
        private void cmbMasterProductList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbMasterProductList.SelectedItem is Product selectedProduct)
            {
                // Tự động điền giá bán từ CSDL (dùng ?. để an toàn nếu Price là null)
                txtUnitPrice.Text = selectedProduct.Price?.ToString("N0");
            }
            else
            {
                txtUnitPrice.Clear();
            }
        }

        // 3. Xử lý nút "Thêm vào phiếu"
        private void btnAddProductToList_Click(object sender, RoutedEventArgs e)
        {
            if (cmbMasterProductList.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn một sản phẩm.");
                return;
            }
            if (!int.TryParse(txtQuantity.Text, out int quantity) || quantity <= 0)
            {
                MessageBox.Show("Số lượng phải là số nguyên dương.");
                return;
            }

            var selectedProduct = (Product)cmbMasterProductList.SelectedItem;
            // Lấy giá từ ô textbox (đã được tự động điền)
            decimal unitPrice = selectedProduct.Price ?? 0; // Lấy giá gốc từ SP

            // Kiểm tra tồn kho ngay tại lúc thêm
            if (quantity > selectedProduct.Quantity)
            {
                MessageBox.Show($"Số lượng yêu cầu ({quantity}) vượt quá tồn kho ({selectedProduct.Quantity}).", "Cảnh báo");
                return;
            }

            var existingItem = _currentExportDetails.Where(item => item != null).FirstOrDefault(item => item.ProductId == selectedProduct.ProductId);

            if (existingItem != null)
            {
                // Tự động cộng dồn số lượng
                if (existingItem.Quantity + quantity > selectedProduct.Quantity)
                {
                    MessageBox.Show($"Tổng số lượng ({existingItem.Quantity + quantity}) vượt quá tồn kho ({selectedProduct.Quantity}).", "Cảnh báo");
                    return;
                }
                existingItem.Quantity += quantity;
                dgProductsToAdd.Items.Refresh();
            }
            else
            {
                // Thêm mới
                var newItem = new StockEntryDetailViewModel
                {
                    ProductId = selectedProduct.ProductId,
                    ProductName = selectedProduct.Name,
                    Quantity = quantity,
                    UnitPrice = unitPrice
                };
                _currentExportDetails.Add(newItem);
            }

            cmbMasterProductList.SelectedIndex = -1;
            txtQuantity.Clear();
            txtUnitPrice.Clear();
        }

        // 4. Xử lý nút "Xóa" (Giống hệt StockEntryView)
        private void btnRemoveItem_Click(object sender, RoutedEventArgs e)
        {
            int productIdToRemove = (int)((Button)sender).Tag;
            var itemToRemove = _currentExportDetails.FirstOrDefault(item => item.ProductId == productIdToRemove);

            if (itemToRemove != null)
            {
                _currentExportDetails.Remove(itemToRemove);
            }
        }

        // 5. Xử lý nút "Gửi Yêu cầu Xuất kho"
        private void btnSaveExport_Click(object sender, RoutedEventArgs e)
        {
            // Kiểm tra User (Lỗi crash "ma")
            if (_currentUser == null)
            {
                MessageBox.Show("LỖI NGHIÊM TRỌNG: Không tìm thấy thông tin người dùng (_currentUser). Vui lòng đăng nhập lại.", "Lỗi Session");
                return;
            }

            // Lọc các item null
            var validDetails = _currentExportDetails.Where(item => item != null).ToList();
            if (validDetails.Count == 0)
            {
                MessageBox.Show("Phiếu xuất phải có ít nhất 1 sản phẩm.");
                return;
            }

            if (MessageBox.Show("Bạn có chắc chắn muốn gửi YÊU CẦU XUẤT KHO này?", "Xác nhận gửi",
                                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                return;
            }

            try
            {
                // Bước 1: Tạo đối tượng Header (Phiếu xuất)
                var orderHeader = new SalesOrder
                {
                    UserId = _currentUser.UserId,
                    SaleDate = DateTime.Now,
                    CustomerName = txtCustomerName.Text, // Lấy tên khách hàng
                    Note = txtNote.Text,
                    TotalAmount = validDetails.Sum(d => d.TotalPrice)
                };

                // Bước 2: Chuyển đổi ViewModel thành Model SalesOrderDetail
                List<SalesOrderDetail> orderDetails = validDetails.Select(vm => new SalesOrderDetail
                {
                    ProductId = vm.ProductId,
                    Quantity = vm.Quantity,
                    UnitPrice = vm.UnitPrice
                }).ToList();

                // Bước 3: Gọi Repository (Hàm "Yêu cầu")
                _salesOrderRepo.RequestSalesOrder(orderHeader, orderDetails);

                MessageBox.Show("Gửi yêu cầu xuất kho thành công! Chờ Admin phê duyệt.", "Thành công");
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gửi yêu cầu thất bại: {ex.Message}", "Lỗi");
            }
        }

        private void ClearForm()
        {
            txtCustomerName.Clear();
            txtNote.Clear();
            _currentExportDetails.Clear();
            cmbMasterProductList.SelectedIndex = -1;
            txtQuantity.Clear();
            txtUnitPrice.Clear();

            // Tải lại danh sách sản phẩm (vì tồn kho có thể đã thay đổi nếu admin duyệt)
            try
            {
                cmbMasterProductList.ItemsSource = _productRepo.GetAll().Where(p => p.Quantity > 0).ToList();
            }
            catch { }
        }
    }
}