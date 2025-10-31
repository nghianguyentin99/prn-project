using Microsoft.EntityFrameworkCore;
using PCShop.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PCShop.Repository
{
    public class CategoryRepository
    {
        private readonly PcshopDbContext _context;

        public CategoryRepository()
        {
            _context = new PcshopDbContext();
        }

        public List<Category> GetAll()
        {
            return _context.Categories.ToList();
        }

        public Category? GetById(int id)
        {
            return _context.Categories.Find(id);
        }

        public void AddCategory(Category category)
        {
            // Kiểm tra trùng tên
            if (_context.Categories.Any(c => c.CategoryName == category.CategoryName))
            {
                throw new Exception("Tên danh mục đã tồn tại.");
            }
            _context.Categories.Add(category);
            _context.SaveChanges();
        }

        public void UpdateCategory(Category category)
        {
            var existing = _context.Categories.Find(category.CategoryId);
            if (existing != null)
            {
                // Kiểm tra trùng tên (trừ chính nó)
                if (_context.Categories.Any(c => c.CategoryName == category.CategoryName && c.CategoryId != category.CategoryId))
                {
                    throw new Exception("Tên danh mục đã tồn tại.");
                }
                existing.CategoryName = category.CategoryName;
                existing.Description = category.Description;
                _context.SaveChanges();
            }
        }

        public void DeleteCategory(int id)
        {
            // Kiểm tra xem danh mục này đã được sử dụng bởi sản phẩm nào chưa
            bool isInUse = _context.Products.Any(p => p.CategoryId == id);
            if (isInUse)
            {
                throw new Exception("Không thể xóa danh mục này. Đã có sản phẩm thuộc danh mục này.");
            }

            var category = _context.Categories.Find(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
                _context.SaveChanges();
            }
        }
    }
}