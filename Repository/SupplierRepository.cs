using PCShop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace PCShop.Repository
{
    public class SupplierRepository
    {
        private readonly PcshopDbContext _context;

        public SupplierRepository()
        {
            _context = new PcshopDbContext();
        }

        public List<Supplier> GetAll()
        {
            return _context.Suppliers.ToList();
        }

        public Supplier? GetById(int id)
        {
            return _context.Suppliers.Find(id);
        }

        public void AddSupplier(Supplier supplier)
        {
            // Validate tên rỗng
            if (string.IsNullOrWhiteSpace(supplier.Name))
                throw new Exception("Tên nhà cung cấp không được để trống.");

            // Validate email
            if (!IsValidEmail(supplier.Email))
                throw new Exception("Email không hợp lệ.");

            // Validate phone
            if (!IsValidPhone(supplier.Phone))
                throw new Exception("Số điện thoại không hợp lệ. Ví dụ hợp lệ: 0912345678");

            // Kiểm tra trùng tên
            if (_context.Suppliers.Any(s => s.Name == supplier.Name))
                throw new Exception("Tên nhà cung cấp đã tồn tại.");

            // Kiểm tra trùng email
            if (_context.Suppliers.Any(s => s.Email == supplier.Email))
                throw new Exception("Email nhà cung cấp đã tồn tại.");
            
            // Kiểm tra trùng số điện thoại
            if (_context.Suppliers.Any(s => s.Phone == supplier.Phone))
                throw new Exception("Số điện thoại nhà cung cấp đã tồn tại.");

            _context.Suppliers.Add(supplier);
            _context.SaveChanges();
        }


        public void UpdateSupplier(Supplier supplier)
        {
            var existing = _context.Suppliers.Find(supplier.SupplierId);
            if (existing != null)
            {
                if (string.IsNullOrWhiteSpace(supplier.Name))
                    throw new Exception("Tên nhà cung cấp không được để trống.");

                if (!IsValidEmail(supplier.Email))
                    throw new Exception("Email không hợp lệ.");

                if (!IsValidPhone(supplier.Phone))
                    throw new Exception("Số điện thoại không hợp lệ.");

                if (_context.Suppliers.Any(s => s.Name == supplier.Name && s.SupplierId != supplier.SupplierId))
                    throw new Exception("Tên nhà cung cấp đã tồn tại.");

                if (_context.Suppliers.Any(s => s.Email == supplier.Email && s.SupplierId != supplier.SupplierId))
                    throw new Exception("Email nhà cung cấp đã tồn tại.");

                existing.Name = supplier.Name;
                existing.Phone = supplier.Phone;
                existing.Address = supplier.Address;
                existing.Email = supplier.Email;
                _context.SaveChanges();
            }
        }


        public void DeleteSupplier(int id)
        {
            // Kiểm tra xem NCC này đã được sử dụng bởi sản phẩm nào chưa
            bool isInUse = _context.Products.Any(p => p.SupplierId == id);
            if (isInUse)
            {
                throw new Exception("Không thể xóa nhà cung cấp này. Đã có sản phẩm được cung cấp bởi nhà cung cấp này.");
            }

            // Tương tự, kiểm tra trong StockEntries (Phiếu nhập)
            bool isInStockEntries = _context.StockEntries.Any(se => se.SupplierId == id);
            if (isInStockEntries)
            {
                throw new Exception("Không thể xóa nhà cung cấp này. Đã có phiếu nhập hàng từ nhà cung cấp này.");
            }

            var supplier = _context.Suppliers.Find(id);
            if (supplier != null)
            {
                _context.Suppliers.Remove(supplier);
                _context.SaveChanges();
            }
        }
        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            // Email chuẩn theo RFC 5322
            return Regex.IsMatch(email,
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                RegexOptions.IgnoreCase);
        }

        private bool IsValidPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return false;

            // Số điện thoại VN: 10 số, bắt đầu từ 03,05,07,08,09
            return Regex.IsMatch(phone,
                @"^(0)\d{9}$");
        }
    }
}