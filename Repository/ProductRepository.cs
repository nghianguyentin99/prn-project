using PCShop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore; // <<< THÊM USING NÀY

namespace PCShop.Repository
{
    public class ProductRepository
    {
        PcshopDbContext context;

        public ProductRepository()
        {
            context = new PcshopDbContext();
        }

        public List<Product> GetAll()
        {
            // Dùng Include để tải thông tin liên quan (Tên danh mục, NCC, Kho)
            return context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .Include(p => p.Warehouse)
                .ToList();
        }
      

        public Product GetById(int id)
        {
            return context.Products.FirstOrDefault(p => p.ProductId == id);
        }
        public void AddProduct(Product product)
        {
            context.Products.Add(product);
            context.SaveChanges();
        }

       
        public void UpdateProduct(Product product)
        {
            var existingProduct = context.Products.Find(product.ProductId);
            if (existingProduct != null)
            {
                // Cập nhật các thuộc tính
                // (Không cập nhật số lượng Quantity ở đây, số lượng sẽ được quản lý bằng phiếu Nhập/Xuất)
                existingProduct.Name = product.Name;
                existingProduct.CategoryId = product.CategoryId;
                existingProduct.SupplierId = product.SupplierId;
                existingProduct.WarehouseId = product.WarehouseId;
                existingProduct.Unit = product.Unit;
                existingProduct.Price = product.Price;
                existingProduct.Description = product.Description;

                context.SaveChanges();
            }
        }
        

        public void DeleteProduct(int id)
        {
            var product = context.Products.FirstOrDefault(p => p.ProductId == id);
            if (product != null)
            {
                // Kiểm tra xem sản phẩm đã có trong phiếu nhập/xuất chi tiết chưa
                bool isInUse = context.StockEntryDetails.Any(d => d.ProductId == id) ||
                               context.SalesOrderDetails.Any(d => d.ProductId == id);

                if (isInUse)
                {
                    throw new Exception("Không thể xóa sản phẩm. Sản phẩm đã tồn tại trong phiếu nhập hoặc phiếu bán.");
                }

                context.Products.Remove(product);
                context.SaveChanges();
            }
        }
    }
}