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
            // === VALIDATION (Kiểm tra dữ liệu) ===
            if (cmbMasterProductList.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn một sản phẩm.");
                return;
            }

            // Đảm bảo selectedProduct là một đối tượng Product hợp lệ
            var selectedProduct = cmbMasterProductList.SelectedItem as Product;
            if (selectedProduct == null)
            {
                MessageBox.Show("Sản phẩm được chọn không hợp lệ.");
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

            // === XỬ LÝ LOGIC ===
            try
            {
                // Kiểm tra xem sản phẩm đã có trong danh sách chưa
                // Dùng .Where để lọc null trước khi tìm (an toàn)
                var existingItem = _currentEntryDetails
                                    .Where(item => item != null)
                                    .FirstOrDefault(item => item.ProductId == selectedProduct.ProductId);

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
                        ProductName = selectedProduct.Name, // Đảm bảo Name không null
                        Quantity = quantity,
                        UnitPrice = unitPrice
                    };

                    // *** SỬA LỖI GỐC ***
                    // Chỉ thêm vào danh sách nếu newItem KHÔNG BỊ NULL
                    if (newItem != null)
                    {
                        _currentEntryDetails.Add(newItem);
                    }
                    else
                    {
                        MessageBox.Show("Không thể tạo chi tiết sản phẩm mới.");
                    }
                }

                // Xóa các ô input
                cmbMasterProductList.SelectedIndex = -1;
                txtQuantity.Clear();
                txtUnitPrice.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi thêm sản phẩm: {ex.Message}");
            }
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
        // 4. Xử lý nút "Lưu Phiếu Nhập" (Nghiệp vụ chính)
        private void btnSaveEntry_Click(object sender, RoutedEventArgs e)
        {
            // === BẪY LỖI NULL MỚI ===
            if (_currentUser == null)
            {
                MessageBox.Show("LỖI NGHIÊM TRỌNG: Không tìm thấy thông tin người dùng (_currentUser). Vui lòng đăng nhập lại.", "Lỗi Session");
                return;
            }

            // === Validations cũ của bạn (vẫn giữ nguyên) ===
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

            // === SỬA LỖI LỌC NULL (RẤT QUAN TRỌNG) ===
            // 1. Lọc bỏ bất kỳ item NULL nào có thể có trong danh sách
            var validDetails = _currentEntryDetails.Where(item => item != null).ToList();

            if (validDetails.Count == 0)
            {
                MessageBox.Show("Phiếu nhập phải có ít nhất 1 sản phẩm hợp lệ.");
                return;
            }
            // (Kết thúc sửa lỗi lọc null)


            if (MessageBox.Show("Bạn có chắc chắn muốn gửi yêu cầu nhập phiếu này?", "Xác nhận gửi",
                                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                return;
            }

            try
            {
                // === SỬA LỖI: Lấy ID từ SelectedItem thay vì SelectedValue ===
                var selectedSupplier = cmbSupplier.SelectedItem as Supplier;
                var selectedWarehouse = cmbWarehouse.SelectedItem as Warehouse;

                if (selectedSupplier == null || selectedWarehouse == null)
                {
                    MessageBox.Show("Không thể lấy thông tin nhà cung cấp hoặc kho. Vui lòng chọn lại.", "Lỗi dữ liệu");
                    return;
                }

                // Bước 1: Tạo đối tượng Header (Phiếu nhập)
                var entryHeader = new StockEntry
                {
                    SupplierId = selectedSupplier.SupplierId,
                    WarehouseId = selectedWarehouse.WarehouseId,
                    UserId = _currentUser.UserId,
                    EntryDate = DateTime.Now,
                    Note = txtNote.Text,
                    TotalAmount = validDetails.Sum(d => d.TotalPrice)
                };

                // Bước 2: Chuyển đổi danh sách ViewModel thành Model
                List<StockEntryDetail> entryDetails = validDetails.Select(vm => vm.ToModel()).ToList();

                // Bước 3: Gọi Repository (đã bao gồm Transaction)
                _stockEntryRepo.RequestStockEntry(entryHeader, entryDetails);

                MessageBox.Show("Gửi yêu cầu nhập kho thành công! Chờ Admin phê duyệt.",
                                "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                // Xóa form để chuẩn bị cho phiếu mới
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gửi yêu cầu thất bại: {ex.Message}", "Lỗi",
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