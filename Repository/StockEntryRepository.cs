using PCShop.Models;
using Microsoft.EntityFrameworkCore; // <-- BẠN CẦN THÊM USING NÀY
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

        // ---------- HÀM DÀNH CHO STAFF (HOẶC BẤT KỲ AI TẠO PHIẾU) ----------

        /// <summary>
        /// Staff tạo một YÊU CẦU nhập kho (Chưa cập nhật tồn kho)
        /// </summary>
        public void RequestStockEntry(StockEntry entryHeader, List<StockEntryDetail> entryDetails)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    // Bước 1: Gán trạng thái "Chờ phê duyệt" (0 = Pending)
                    entryHeader.Status = 0;

                    // Lưu Header để lấy EntryID
                    _context.StockEntries.Add(entryHeader);
                    _context.SaveChanges();

                    // Bước 2: Lưu Details
                    foreach (var detail in entryDetails)
                    {
                        detail.EntryId = entryHeader.EntryId;
                        _context.StockEntryDetails.Add(detail);
                    }
                    _context.SaveChanges();

                    // --- QUAN TRỌNG ---
                    // KHÔNG CẬP NHẬT Product.Quantity
                    // KHÔNG TẠO StockMovement
                    // Vì phiếu chưa được duyệt

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new Exception("Gửi yêu cầu nhập kho thất bại: " + ex.Message);
                }
            }
        }

        // ---------- CÁC HÀM DÀNH CHO ADMIN PHÊ DUYỆT ----------

        /// <summary>
        /// Lấy tất cả các phiếu đang chờ duyệt (Dùng cho Admin View)
        /// </summary>
        public List<StockEntry> GetPendingEntries()
        {
            // Lấy các phiếu có Status = 0
            return _context.StockEntries
                .Include(e => e.Supplier) // Lấy tên Nhà cung cấp
                .Include(e => e.User)     // Lấy tên người tạo
                .Where(e => e.Status == 0)
                .ToList();
        }

        /// <summary>
        /// ADMIN: Phê duyệt một yêu cầu nhập kho.
        /// Đây là nơi TỒN KHO THỰC SỰ TĂNG LÊN.
        /// </summary>
        public void ApproveStockEntry(int entryId, int adminUserId)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    // Bước 1: Tìm phiếu nhập VÀ các chi tiết của nó
                    var entry = _context.StockEntries
                                        .Include(e => e.StockEntryDetails) // Rất quan trọng
                                        .FirstOrDefault(e => e.EntryId == entryId);

                    if (entry == null) throw new Exception("Không tìm thấy phiếu nhập.");
                    if (entry.Status != 0) throw new Exception("Phiếu này đã được xử lý (không còn ở trạng thái Chờ).");

                    // Bước 2: Chuyển trạng thái sang "Đã phê duyệt"
                    entry.Status = 1; // 1 = Approved
                    entry.ApprovedByUserId = adminUserId;
                    entry.ApprovedDate = DateTime.Now;

                    // Bước 3: THỰC THI nghiệp vụ (Code này trước đây nằm ở hàm Create)
                    foreach (var detail in entry.StockEntryDetails)
                        {
                        // 3a. Cập nhật (tăng) số lượng tồn kho
                        var product = _context.Products.Find(detail.ProductId);
                        if (product == null) throw new Exception($"Không tìm thấy SP ID: {detail.ProductId}");

                        // CỘNG TỒN KHO
                        product.Quantity += detail.Quantity ?? 0; // (Dùng ?? 0 nếu Quantity trong detail là int?)

                        // 3b. Ghi lại lịch sử biến động kho
                        var movement = new StockMovement
                        {
                            ProductId = detail.ProductId,
                            WarehouseId = entry.WarehouseId,
                            Type = "Import",
                            Quantity = detail.Quantity,
                            Date = DateTime.Now,
                            RelatedId = entry.EntryId, // Liên kết tới phiếu nhập
                            UserId = adminUserId // Ghi lại Admin đã duyệt
                        };
                        _context.StockMovements.Add(movement);
                    }

                    // Bước 4: Lưu tất cả thay đổi
                    _context.SaveChanges();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new Exception("Phê duyệt phiếu nhập thất bại: " + ex.Message);
                }
            }
        }

        /// <summary>
        /// ADMIN: Từ chối một yêu cầu nhập kho
        /// </summary>
        public void RejectStockEntry(int entryId, int adminUserId)
        {
            var entry = _context.StockEntries.Find(entryId);
            if (entry == null) throw new Exception("Không tìm thấy phiếu nhập.");
            if (entry.Status != 0) throw new Exception("Phiếu này đã được xử lý.");

            // Chỉ cần cập nhật trạng thái
            entry.Status = 2; // 2 = Rejected
            entry.ApprovedByUserId = adminUserId; // Ghi lại ai đã từ chối
            entry.ApprovedDate = DateTime.Now;
            _context.SaveChanges();
        }
        /// <summary>
        /// Lấy chi tiết (sản phẩm) của một phiếu nhập, KÈM THEO thông tin Product
        /// </summary>
        public List<StockEntryDetail> GetDetailsForEntry(int entryId)
        {
            return _context.StockEntryDetails
                .Include(d => d.Product) // <-- Lấy kèm thông tin Product (tên, v.v...)
                .Where(d => d.EntryId == entryId)
                .ToList();
        }
    }
}