using Microsoft.EntityFrameworkCore; // Phải thêm using này
using PCShop.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PCShop.Repository
{
    public class SalesOrderRepository
    {
        private readonly PcshopDbContext _context;

        public SalesOrderRepository()
        {
            _context = new PcshopDbContext();
        }

        // ---------- HÀM DÀNH CHO STAFF (TẠO YÊU CẦU) ----------

        /// <summary>
        /// Staff tạo một YÊU CẦU xuất kho (Chưa trừ tồn kho)
        /// </summary>
        public void RequestSalesOrder(SalesOrder orderHeader, List<SalesOrderDetail> orderDetails)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    // Bước 1: Gán trạng thái "Chờ phê duyệt" (0 = Pending)
                    orderHeader.Status = 0;
                    _context.SalesOrders.Add(orderHeader);
                    _context.SaveChanges(); // Lưu để lấy SalesID

                    // Bước 2: Lưu Details
                    foreach (var detail in orderDetails)
                    {
                        detail.SalesId = orderHeader.SalesId;
                        _context.SalesOrderDetails.Add(detail);
                    }
                    _context.SaveChanges();

                    // KHÔNG KIỂM TRA TỒN KHO
                    // KHÔNG TRỪ TỒN KHO
                    // KHÔNG TẠO STOCKMOVEMENT

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new Exception("Gửi yêu cầu xuất kho thất bại: " + ex.Message);
                }
            }
        }

        // ---------- CÁC HÀM DÀNH CHO ADMIN PHÊ DUYỆT ----------

        /// <summary>
        /// Lấy tất cả các phiếu xuất đang chờ duyệt (Dùng cho Admin View)
        /// </summary>
        public List<SalesOrder> GetPendingSalesOrders()
        {
            return _context.SalesOrders
                .Include(o => o.User) // Lấy tên người tạo
                .Where(o => o.Status == 0)
                .ToList();
        }

        /// <summary>
        /// Lấy chi tiết (sản phẩm) của một phiếu xuất
        /// </summary>
        public List<SalesOrderDetail> GetDetailsForSalesOrder(int salesId)
        {
            return _context.SalesOrderDetails
                .Include(d => d.Product) // Lấy kèm thông tin Product
                .Where(d => d.SalesId == salesId)
                .ToList();
        }


        /// <summary>
        /// ADMIN: Phê duyệt một yêu cầu xuất kho.
        /// Đây là nơi TỒN KHO THỰC SỰ BỊ TRỪ.
        /// </summary>
        public void ApproveSalesOrder(int salesId, int adminUserId)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var order = _context.SalesOrders
                                        .Include(o => o.SalesOrderDetails) // Lấy kèm chi tiết
                                        .FirstOrDefault(o => o.SalesId == salesId);

                    if (order == null) throw new Exception("Không tìm thấy phiếu xuất.");
                    if (order.Status != 0) throw new Exception("Phiếu này đã được xử lý.");

                    // === BƯỚC QUAN TRỌNG NHẤT: KIỂM TRA TỒN KHO ===
                    foreach (var detail in order.SalesOrderDetails)
                    {
                        var product = _context.Products.Find(detail.ProductId);
                        if (product == null)
                        {
                            throw new Exception($"Không tìm thấy SP ID: {detail.ProductId}");
                        }
                        if (product.Quantity < detail.Quantity)
                        {
                            throw new Exception($"Không đủ hàng! Sản phẩm '{product.Name}' chỉ còn {product.Quantity} (cần {detail.Quantity}).");
                        }
                    }

                    // Nếu code chạy đến đây, tức là kho đủ hàng
                    // === BƯỚC 2: TRỪ TỒN KHO VÀ GHI LỊCH SỬ ===
                    foreach (var detail in order.SalesOrderDetails)
                    {
                        // 2a. Trừ tồn kho
                        var product = _context.Products.Find(detail.ProductId);
                        product.Quantity -= detail.Quantity;

                        // 2b. Ghi lại lịch sử (Ghi số ÂM để thống kê)
                        var movement = new StockMovement
                        {
                            ProductId = detail.ProductId,
                            WarehouseId = product.WarehouseId, // Lấy kho từ sản phẩm
                            Type = "Export",
                            Quantity = -detail.Quantity, // Ghi số ÂM
                            Date = DateTime.Now,
                            RelatedId = order.SalesId, // Liên kết tới phiếu xuất
                            UserId = adminUserId
                        };
                        _context.StockMovements.Add(movement);
                    }

                    // Bước 3: Cập nhật trạng thái phiếu xuất
                    order.Status = 1; // 1 = Approved
                    order.ApprovedByUserId = adminUserId;
                    order.ApprovedDate = DateTime.Now;

                    _context.SaveChanges();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new Exception("Phê duyệt phiếu xuất thất bại: " + ex.Message);
                }
            }
        }

        /// <summary>
        /// ADMIN: Từ chối một yêu cầu xuất kho
        /// </summary>
        public void RejectSalesOrder(int salesId, int adminUserId)
        {
            var order = _context.SalesOrders.Find(salesId);
            if (order == null) throw new Exception("Không tìm thấy phiếu xuất.");
            if (order.Status != 0) throw new Exception("Phiếu này đã được xử lý.");

            order.Status = 2; // 2 = Rejected
            order.ApprovedByUserId = adminUserId;
            order.ApprovedDate = DateTime.Now;
            _context.SaveChanges();
        }
    }
}