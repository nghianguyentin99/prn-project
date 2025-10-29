using PCShop.Models;
using PCShop.Repository;
using System;
using System.Windows;
using System.Windows.Controls;

namespace PCShop.View.Admin
{
    /// <summary>
    /// Interaction logic for SupplierManagementView.xaml
    /// </summary>
    public partial class SupplierManagementView : UserControl
    {
        private readonly SupplierRepository _supplierRepository;

        public SupplierManagementView()
        {
            InitializeComponent();
            _supplierRepository = new SupplierRepository();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSuppliers();
        }

        private void LoadSuppliers()
        {
            try
            {
                dgSuppliers.ItemsSource = _supplierRepository.GetAll();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải danh sách nhà cung cấp: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearForm()
        {
            txtSupplierId.Text = string.Empty;
            txtName.Text = string.Empty;
            txtPhone.Text = string.Empty;
            txtEmail.Text = string.Empty;
            txtAddress.Text = string.Empty;
            dgSuppliers.SelectedIndex = -1;
        }

        private void dgSuppliers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgSuppliers.SelectedItem is Supplier selected)
            {
                txtSupplierId.Text = selected.SupplierId.ToString();
                txtName.Text = selected.Name;
                txtPhone.Text = selected.Phone;
                txtEmail.Text = selected.Email;
                txtAddress.Text = selected.Address;
            }
        }

        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Tên nhà cung cấp là bắt buộc.", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                Supplier newSupplier = new Supplier
                {
                    Name = txtName.Text.Trim(),
                    Phone = txtPhone.Text.Trim(),
                    Email = txtEmail.Text.Trim(),
                    Address = txtAddress.Text.Trim()
                };

                _supplierRepository.AddSupplier(newSupplier);
                MessageBox.Show("Thêm nhà cung cấp thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadSuppliers();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi thêm nhà cung cấp: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (dgSuppliers.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn một nhà cung cấp để cập nhật.", "Chưa chọn", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Tên nhà cung cấp là bắt buộc.", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                Supplier updatedSupplier = new Supplier
                {
                    SupplierId = int.Parse(txtSupplierId.Text),
                    Name = txtName.Text.Trim(),
                    Phone = txtPhone.Text.Trim(),
                    Email = txtEmail.Text.Trim(),
                    Address = txtAddress.Text.Trim()
                };

                _supplierRepository.UpdateSupplier(updatedSupplier);
                MessageBox.Show("Cập nhật nhà cung cấp thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadSuppliers();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi cập nhật nhà cung cấp: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgSuppliers.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn một nhà cung cấp để xóa.", "Chưa chọn", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show("Bạn có chắc chắn muốn xóa nhà cung cấp này?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
            {
                return;
            }

            try
            {
                int supplierId = int.Parse(txtSupplierId.Text);
                _supplierRepository.DeleteSupplier(supplierId);
                MessageBox.Show("Xóa nhà cung cấp thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadSuppliers();
                ClearForm();
            }
            catch (Exception ex)
            {
                // Bắt lỗi từ Repository (khi xóa NCC đang có sản phẩm/phiếu nhập)
                MessageBox.Show($"Lỗi khi xóa nhà cung cấp: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}