using PCShop.Models;
using PCShop.Repository;
using System;
using System.Windows;
using System.Windows.Controls;

namespace PCShop.View
{
    /// <summary>
    /// Interaction logic for ProductManagementView.xaml
    /// </summary>
    public partial class ProductManagementView : UserControl
    {
        private readonly ProductRepository _productRepo;
        private readonly CategoryRepository _categoryRepo;
        private readonly SupplierRepository _supplierRepo;
        private readonly WarehouseRepository _warehouseRepo;

        public ProductManagementView()
        {
            InitializeComponent();
            _productRepo = new ProductRepository();
            _categoryRepo = new CategoryRepository();
            _supplierRepo = new SupplierRepository();
            _warehouseRepo = new WarehouseRepository();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            LoadProducts();
            LoadComboBoxes();
        }

        private void LoadProducts()
        {
            try
            {
                dgProducts.ItemsSource = _productRepo.GetAll();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải danh sách sản phẩm: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadComboBoxes()
        {
            try
            {
                // Tải dữ liệu cho 3 ComboBox
                cmbCategory.ItemsSource = _categoryRepo.GetAll();
                cmbSupplier.ItemsSource = _supplierRepo.GetAll();
                cmbWarehouse.ItemsSource = _warehouseRepo.GetAll();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải dữ liệu ComboBox (Danh mục, NCC, Kho): {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearForm()
        {
            txtProductID.Text = string.Empty;
            txtName.Text = string.Empty;
            txtQuantity.Text = "0";
            txtUnit.Text = "Cái";
            txtPrice.Text = "0";
            txtDescription.Text = string.Empty;
            cmbCategory.SelectedIndex = -1;
            cmbSupplier.SelectedIndex = -1;
            cmbWarehouse.SelectedIndex = -1;
            dgProducts.SelectedIndex = -1;
        }

        private Product GetProductFromForm()
        {
            // Lấy dữ liệu từ form để chuẩn bị Add/Update
            try
            {
                if (cmbCategory.SelectedValue == null || cmbSupplier.SelectedValue == null || cmbWarehouse.SelectedValue == null)
                {
                    MessageBox.Show("Vui lòng chọn đầy đủ Danh mục, Nhà cung cấp và Kho.", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return null;
                }

                return new Product
                {
                    ProductId = string.IsNullOrEmpty(txtProductID.Text) ? 0 : int.Parse(txtProductID.Text),
                    Name = txtName.Text.Trim(),
                    Unit = txtUnit.Text.Trim(),
                    Price = decimal.Parse(txtPrice.Text),
                    Description = txtDescription.Text.Trim(),
                    CategoryId = (int)cmbCategory.SelectedValue,
                    SupplierId = (int)cmbSupplier.SelectedValue,
                    WarehouseId = (int)cmbWarehouse.SelectedValue,
                    Quantity = 0 // Số lượng ban đầu khi thêm mới là 0, sẽ được cập nhật khi nhập kho
                };
            }
            catch (FormatException)
            {
                MessageBox.Show("Vui lòng nhập đúng định dạng số cho Giá bán.", "Lỗi Dữ Liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi lấy dữ liệu từ form: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtUnit.Text))
            {
                MessageBox.Show("Tên sản phẩm và Đơn vị tính là bắt buộc.", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Product product = GetProductFromForm();
            if (product == null) return; // Lỗi đã được thông báo trong GetProductFromForm

            try
            {
                _productRepo.AddProduct(product);
                MessageBox.Show("Thêm sản phẩm thành công! \n(Số lượng tồn kho ban đầu là 0, vui lòng tạo phiếu nhập kho).", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadProducts();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi thêm sản phẩm: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (dgProducts.SelectedItem == null || string.IsNullOrEmpty(txtProductID.Text))
            {
                MessageBox.Show("Vui lòng chọn một sản phẩm để cập nhật.", "Chưa chọn sản phẩm", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtUnit.Text))
            {
                MessageBox.Show("Tên sản phẩm và Đơn vị tính là bắt buộc.", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Product product = GetProductFromForm();
            if (product == null) return;

            try
            {
                _productRepo.UpdateProduct(product);
                MessageBox.Show("Cập nhật thông tin sản phẩm thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadProducts();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi cập nhật sản phẩm: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgProducts.SelectedItem == null || string.IsNullOrEmpty(txtProductID.Text))
            {
                MessageBox.Show("Vui lòng chọn một sản phẩm để xóa.", "Chưa chọn sản phẩm", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show("Bạn có chắc chắn muốn xóa (ngừng kinh doanh) sản phẩm này?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
            {
                return;
            }

            try
            {
                int productId = int.Parse(txtProductID.Text);
                _productRepo.DeleteProduct(productId);
                MessageBox.Show("Xóa sản phẩm thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadProducts();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xóa sản phẩm: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void dgProducts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgProducts.SelectedItem is Product selectedProduct)
            {
                // Điền dữ liệu từ DataGrid vào Form
                txtProductID.Text = selectedProduct.ProductId.ToString();
                txtName.Text = selectedProduct.Name;
                txtQuantity.Text = selectedProduct.Quantity.ToString();
                txtUnit.Text = selectedProduct.Unit;
                txtPrice.Text = selectedProduct.Price.ToString();
                txtDescription.Text = selectedProduct.Description;

                // Chọn đúng giá trị trong ComboBox
                cmbCategory.SelectedValue = selectedProduct.CategoryId;
                cmbSupplier.SelectedValue = selectedProduct.SupplierId;
                cmbWarehouse.SelectedValue = selectedProduct.WarehouseId;
            }
        }
    }
}