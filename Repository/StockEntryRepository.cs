// Repository/StockEntryRepository.cs
using PCShop.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PCShop.Repository
{
    public class StockEntryRepository
    {
        private readonly PcshopDbContext _context;

        public StockEntryRepository()
        {
            _context = new PcshopDbContext();
        }

        /// <summary>
        /// Tạo một phiếu nhập kho mới (Nghiệp vụ Nhập)
        /// </summary>
        /// <param name="entry">Thông tin chung của phiếu nhập</param>
        /// <param name="details">Danh sách chi tiết sản phẩm</param>
        /// <param name="userId">ID người tạo phiếu</param>
        public void CreateStockEntry(StockEntry entry, List<StockEntryDetail> details)
        {
            // Bọc toàn bộ nghiệp vụ trong một Transaction
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    // Bước 1: Lưu phiếu nhập (header) để lấy EntryID
                    _context.StockEntries.Add(entry);
                    _context.SaveChanges(); // Lưu để EF gán EntryID tự tăng

                    // Lấy EntryID vừa được tạo
                    int newEntryId = entry.EntryId;

                    foreach (var detail in details)
                    {
                        // Bước 2: Gán EntryID cho từng chi tiết
                        detail.EntryId = newEntryId;
                        _context.StockEntryDetails.Add(detail);

                        // Bước 3: Cập nhật (tăng) số lượng tồn kho trong bảng Products
                        var product = _context.Products.Find(detail.ProductId);
                        if (product == null)
                        {
                            throw new Exception($"Không tìm thấy sản phẩm ID: {detail.ProductId}");
                        }

                        // Cộng dồn số lượng
                        product.Quantity += detail.Quantity;

                        // Bước 4: Ghi lại lịch sử biến động kho
                        var movement = new StockMovement
                        {
                            ProductId = detail.ProductId,
                            WarehouseId = entry.WarehouseId, // Lấy kho từ phiếu nhập
                            Type = "Import",
                            Quantity = detail.Quantity, // Số dương
                            Date = DateTime.Now,
                            RelatedId = newEntryId, // Liên kết tới phiếu nhập
                            UserId = entry.UserId
                        };
                        _context.StockMovements.Add(movement);
                    }

                    // Bước 5: Lưu tất cả thay đổi (Details, Product.Quantity, Movements)
                    _context.SaveChanges();

                    // Bước 6: Nếu mọi thứ thành công, commit transaction
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    // Nếu có lỗi, rollback tất cả
                    transaction.Rollback();
                    throw new Exception("Tạo phiếu nhập thất bại. " + ex.Message);
                }
            }
        }
    }
}