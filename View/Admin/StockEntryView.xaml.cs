using PCShop.Models;
using PCShop.Repository;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel; // Cần dùng ObservableCollection
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace PCShop.View
{
    public partial class StockEntryView : UserControl
    {
        // View Model nội bộ để hiển thị DataGrid (có Tên sản phẩm)
        public class StockEntryDetailViewModel
        {
            public int ProductId { get; set; }
            public string ProductName { get; set; } // Thuộc tính thêm
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal TotalPrice => Quantity * UnitPrice;

            // Chuyển đổi từ ViewModel về Model gốc khi lưu
            public StockEntryDetail ToModel()
            {
                return new StockEntryDetail
                {
                    ProductId = this.ProductId,
                    Quantity = this.Quantity,
                    UnitPrice = this.UnitPrice
                };
            }
        }

        // Repositories
        private readonly SupplierRepository _supplierRepo;
        private readonly WarehouseRepository _warehouseRepo;
        private readonly ProductRepository _productRepo;
        private readonly StockEntryRepository _stockEntryRepo;
        private readonly User _currentUser; // Giả sử bạn truyền User vào

        // Danh sách chi tiết cho phiếu nhập hiện tại
        private ObservableCollection<StockEntryDetailViewModel> _currentEntryDetails;

        // Giả sử bạn khởi tạo View này và truyền User vào
        public StockEntryView(User currentUser)
        {
            InitializeComponent();

            _supplierRepo = new SupplierRepository();
            _warehouseRepo = new WarehouseRepository();
            _productRepo = new ProductRepository();
            _stockEntryRepo = new StockEntryRepository();
            _currentUser = currentUser;

            _currentEntryDetails = new ObservableCollection<StockEntryDetailViewModel>();
            dgProductsToAdd.ItemsSource = _currentEntryDetails;
        }

        // 1. Tải các ComboBox khi View được mở
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                cmbSupplier.ItemsSource = _supplierRepo.GetAll();
                cmbWarehouse.ItemsSource = _warehouseRepo.GetAll();
                cmbMasterProductList.ItemsSource = _productRepo.GetAll();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải dữ liệu ban đầu: {ex.Message}");
            }
        }

        // 2. Xử lý nút "Thêm vào phiếu"
        private void btnAddProductToList_Click(object sender, RoutedEventArgs e)
        {
            // Validations
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
            if (!decimal.TryParse(txtUnitPrice.Text, out decimal unitPrice) || unitPrice < 0)
            {
                MessageBox.Show("Đơn giá phải là một số hợp lệ.");
                return;
            }

            var selectedProduct = (Product)cmbMasterProductList.SelectedItem;

            // Kiểm tra xem sản phẩm đã có trong danh sách chưa
            var existingItem = _currentEntryDetails.FirstOrDefault(item => item.ProductId == selectedProduct.ProductId);

            if (existingItem != null)
            {
                // Nếu đã có -> Cập nhật (hỏi người dùng)
                if (MessageBox.Show("Sản phẩm này đã có trong phiếu. Bạn muốn cập nhật số lượng và đơn giá không?",
                                    "Xác nhận cập nhật", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    existingItem.Quantity = quantity;
                    existingItem.UnitPrice = unitPrice;
                    // Cập nhật lại DataGrid
                    dgProductsToAdd.Items.Refresh();
                }
            }
            else
            {
                // Nếu chưa có -> Thêm mới
                var newItem = new StockEntryDetailViewModel
                {
                    ProductId = selectedProduct.ProductId,
                    ProductName = selectedProduct.Name,
                    Quantity = quantity,
                    UnitPrice = unitPrice
                };
                _currentEntryDetails.Add(newItem);
            }

            // Xóa các ô input
            cmbMasterProductList.SelectedIndex = -1;
            txtQuantity.Clear();
            txtUnitPrice.Clear();
        }

        // 3. Xử lý nút "Xóa" trong DataGrid (Tùy chọn)
        private void btnRemoveItem_Click(object sender, RoutedEventArgs e)
        {
            // Lấy ProductId từ Tag của Button
            int productIdToRemove = (int)((Button)sender).Tag;
            var itemToRemove = _currentEntryDetails.FirstOrDefault(item => item.ProductId == productIdToRemove);

            if (itemToRemove != null)
            {
                _currentEntryDetails.Remove(itemToRemove);
            }
        }

        // 4. Xử lý nút "Lưu Phiếu Nhập" (Nghiệp vụ chính)
        private void btnSaveEntry_Click(object sender, RoutedEventArgs e)
        {
            // Validations
            if (cmbSupplier.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn nhà cung cấp.");
                return;
            }
            if (cmbWarehouse.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn kho nhập.");
                return;
            }
            if (_currentEntryDetails.Count == 0)
            {
                MessageBox.Show("Phiếu nhập phải có ít nhất 1 sản phẩm.");
                return;
            }

            if (MessageBox.Show("Bạn có chắc chắn muốn lưu phiếu nhập này?", "Xác nhận lưu",
                                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                return;
            }

            try
            {
                // Bước 1: Tạo đối tượng Header (Phiếu nhập)
                var entryHeader = new StockEntry
                {
                    SupplierId = (int)cmbSupplier.SelectedValue,
                    WarehouseId = (int)cmbWarehouse.SelectedValue,
                    UserId = _currentUser.UserId, // Lấy từ user đăng nhập
                    EntryDate = DateTime.Now,
                    Note = txtNote.Text,
                    // Tính tổng tiền cho phiếu (nếu CSDL của bạn có)
                    TotalAmount = _currentEntryDetails.Sum(d => d.TotalPrice)
                };

                // Bước 2: Chuyển đổi danh sách ViewModel thành Model
                List<StockEntryDetail> entryDetails = _currentEntryDetails.Select(vm => vm.ToModel()).ToList();

                // Bước 3: Gọi Repository (đã bao gồm Transaction)
                _stockEntryRepo.CreateStockEntry(entryHeader, entryDetails);

                MessageBox.Show("Lưu phiếu nhập kho thành công!", "Thành công",
                                MessageBoxButton.OK, MessageBoxImage.Information);

                // Xóa form để chuẩn bị cho phiếu mới
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lưu phiếu thất bại: {ex.Message}", "Lỗi",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearForm()
        {
            cmbSupplier.SelectedIndex = -1;
            cmbWarehouse.SelectedIndex = -1;
            txtNote.Clear();
            _currentEntryDetails.Clear();
            cmbMasterProductList.SelectedIndex = -1;
            txtQuantity.Clear();
            txtUnitPrice.Clear();
        }
    }
}