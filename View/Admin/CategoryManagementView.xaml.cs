using PCShop.Models;
using PCShop.Repository;
using System;
using System.Windows;
using System.Windows.Controls;

namespace PCShop.View.Admin
{
    /// <summary>
    /// Interaction logic for CategoryManagementView.xaml
    /// </summary>
    public partial class CategoryManagementView : UserControl
    {
        private readonly CategoryRepository _categoryRepository;

        public CategoryManagementView()
        {
            InitializeComponent();
            _categoryRepository = new CategoryRepository();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            LoadCategories();
        }

        private void LoadCategories()
        {
            try
            {
                dgCategories.ItemsSource = _categoryRepository.GetAll();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải danh sách danh mục: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearForm()
        {
            txtCategoryId.Text = string.Empty;
            txtCategoryName.Text = string.Empty;
            txtDescription.Text = string.Empty;
            dgCategories.SelectedIndex = -1;
        }

        private void dgCategories_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgCategories.SelectedItem is Category selectedCategory)
            {
                txtCategoryId.Text = selectedCategory.CategoryId.ToString();
                txtCategoryName.Text = selectedCategory.CategoryName;
                txtDescription.Text = selectedCategory.Description;
            }
        }

        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCategoryName.Text))
            {
                MessageBox.Show("Tên danh mục là bắt buộc.", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                Category newCategory = new Category
                {
                    CategoryName = txtCategoryName.Text.Trim(),
                    Description = txtDescription.Text.Trim()
                };

                _categoryRepository.AddCategory(newCategory);
                MessageBox.Show("Thêm danh mục thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadCategories();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi thêm danh mục: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (dgCategories.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn một danh mục để cập nhật.", "Chưa chọn", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtCategoryName.Text))
            {
                MessageBox.Show("Tên danh mục là bắt buộc.", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                Category updatedCategory = new Category
                {
                    CategoryId = int.Parse(txtCategoryId.Text),
                    CategoryName = txtCategoryName.Text.Trim(),
                    Description = txtDescription.Text.Trim()
                };

                _categoryRepository.UpdateCategory(updatedCategory);
                MessageBox.Show("Cập nhật danh mục thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadCategories();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi cập nhật danh mục: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgCategories.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn một danh mục để xóa.", "Chưa chọn", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show("Bạn có chắc chắn muốn xóa danh mục này?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
            {
                return;
            }

            try
            {
                int categoryId = int.Parse(txtCategoryId.Text);
                _categoryRepository.DeleteCategory(categoryId);
                MessageBox.Show("Xóa danh mục thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadCategories();
                ClearForm();
            }
            catch (Exception ex)
            {
                // Bắt lỗi từ Repository (khi xóa danh mục đang có sản phẩm)
                MessageBox.Show($"Lỗi khi xóa danh mục: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}