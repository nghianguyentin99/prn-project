using PCShop.Models;
using PCShop.Repository;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using PCShop.View; 

namespace PCShop.View.Staff
{
   
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
            if (_currentUser == null)
            {
                MessageBox.Show("Lỗi phiên làm việc. Vui lòng đăng nhập lại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // --- KIỂM TRA BẮT BUỘC ---
            if (string.IsNullOrWhiteSpace(txtCustomerName.Text))
            {
                MessageBox.Show("Bạn chưa nhập [Tên khách hàng].", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtCustomerName.Focus();
                return;
            }

            var validDetails = _currentExportDetails.Where(item => item != null).ToList();
            if (validDetails.Count == 0)
            {
                MessageBox.Show("Phiếu xuất phải có ít nhất 1 sản phẩm.", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                cmbMasterProductList.Focus();
                return;
            }
            // --------------------------

            if (MessageBox.Show("Gửi yêu cầu xuất kho này cho Admin duyệt?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                return;
            }

            try
            {
                var orderHeader = new SalesOrder
                {
                    UserId = _currentUser.UserId,
                    SaleDate = DateTime.Now,
                    CustomerName = txtCustomerName.Text.Trim(),
                    Note = txtNote.Text?.Trim(),
                    TotalAmount = validDetails.Sum(d => d.TotalPrice)
                };

                List<SalesOrderDetail> orderDetails = validDetails.Select(vm => new SalesOrderDetail
                {
                    ProductId = vm.ProductId,
                    Quantity = vm.Quantity,
                    UnitPrice = vm.UnitPrice
                }).ToList();

                _salesOrderRepo.RequestSalesOrder(orderHeader, orderDetails);

                MessageBox.Show("Đã gửi yêu cầu thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi gửi yêu cầu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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