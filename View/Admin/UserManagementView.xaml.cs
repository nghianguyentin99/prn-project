using PCShop.Models;
using PCShop.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace PCShop.View.Admin
{
    /// <summary>
    /// Interaction logic for UserManagementView.xaml
    /// </summary>
    public partial class UserManagementView : UserControl
    {
        private readonly UserRepository _userRepository;

        public UserManagementView()
        {
            InitializeComponent();
            _userRepository = new UserRepository();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Tải danh sách người dùng khi mở view
            LoadUsers();
        }

        private void LoadUsers()
        {
            try
            {
                dgUsers.ItemsSource = _userRepository.GetAll();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải danh sách người dùng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearForm()
        {
            txtUserId.Text = string.Empty;
            txtUsername.Text = string.Empty;
            txtFullName.Text = string.Empty;
            txtPassword.Password = string.Empty;
            cmbRole.SelectedIndex = 1; // Mặc định là Staff
            dgUsers.SelectedIndex = -1; // Bỏ chọn trên DataGrid
        }

        private void dgUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Khi chọn một dòng trong DataGrid, điền thông tin vào form
            if (dgUsers.SelectedItem is User selectedUser)
            {
                txtUserId.Text = selectedUser.UserId.ToString();
                txtUsername.Text = selectedUser.Username;
                txtFullName.Text = selectedUser.FullName;
                cmbRole.SelectedValue = selectedUser.Role == 0 ? cmbRole.Items[0] : cmbRole.Items[1];
                txtPassword.Password = string.Empty; // Không hiển thị mật khẩu cũ
            }
        }

        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            // Kiểm tra dữ liệu đầu vào
            if (string.IsNullOrWhiteSpace(txtUsername.Text) ||
                string.IsNullOrWhiteSpace(txtPassword.Password) ||
                cmbRole.SelectedItem == null)
            {
                MessageBox.Show("Tên đăng nhập, Mật khẩu, và Quyền là bắt buộc khi thêm mới.", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                User newUser = new User
                {
                    Username = txtUsername.Text.Trim(),
                    FullName = txtFullName.Text.Trim(),
                    Password = txtPassword.Password, // Lưu plain text (theo thiết kế DB)
                    Role = int.Parse(((ComboBoxItem)cmbRole.SelectedItem).Tag.ToString())
                };

                _userRepository.AddUser(newUser);
                MessageBox.Show("Thêm tài khoản thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadUsers();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi thêm tài khoản: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (dgUsers.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn một tài khoản để cập nhật.", "Chưa chọn", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtUsername.Text) || cmbRole.SelectedItem == null)
            {
                MessageBox.Show("Tên đăng nhập và Quyền không được để trống.", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                User updatedUser = new User
                {
                    UserId = int.Parse(txtUserId.Text),
                    Username = txtUsername.Text.Trim(),
                    FullName = txtFullName.Text.Trim(),
                    Password = txtPassword.Password, // Repository sẽ kiểm tra nếu rỗng thì không đổi
                    Role = int.Parse(((ComboBoxItem)cmbRole.SelectedItem).Tag.ToString())
                };

                _userRepository.UpdateUser(updatedUser);
                MessageBox.Show("Cập nhật tài khoản thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadUsers();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi cập nhật tài khoản: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgUsers.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn một tài khoản để xóa.", "Chưa chọn", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show("Bạn có chắc chắn muốn xóa (vô hiệu hóa) tài khoản này?\nThao tác này không thể hoàn tác.", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
            {
                return;
            }

            try
            {
                int userId = int.Parse(txtUserId.Text);
                _userRepository.DeleteUser(userId);
                MessageBox.Show("Xóa tài khoản thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadUsers();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xóa tài khoản: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    /// <summary>
    /// Converter để hiển thị 0 -> "Admin", 1 -> "Staff" trong DataGrid
    /// </summary>
    public class RoleToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int role)
            {
                return role == 0 ? "Admin" : "Staff";
            }
            return "Staff";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class ActiveToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool active)
            {
                return active == true ? "Hoạt động" : "Bị vô hiệu hóa";
            }
            return "Không rõ";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}