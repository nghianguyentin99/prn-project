using PCShop.Models;
using System;
using System.Linq;

namespace PCShop.Repository
{
    public class InventoryRepository
    {
        private readonly PcshopDbContext _context;

        public InventoryRepository()
        {
            _context = new PcshopDbContext();
        }

        /// <summary>
        /// Cập nhật Mức tồn kho tối thiểu (Nghiệp vụ C.4)
        /// </summary>
        public void UpdateMinStockLevel(int productId, int minStockLevel)
        {
            var product = _context.Products.Find(productId);
            if (product != null)
            {
                product.MinStockLevel = minStockLevel;
                _context.SaveChanges();
            }
            else
            {
                throw new Exception("Không tìm thấy sản phẩm.");
            }
        }

        /// <summary>
        /// Điều chỉnh số lượng tồn kho (Nghiệp vụ C.1 & C.2)
        /// </summary>
        public void AdjustStock(int productId, int actualNewQuantity, int userId)
        {
            // Bắt đầu Transaction để đảm bảo an toàn dữ liệu
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var product = _context.Products.Find(productId);
                    if (product == null)
                    {
                        throw new Exception("Không tìm thấy sản phẩm.");
                    }

                    // (Giả sử model Product.Quantity là int (non-null) khớp với CSDL)
                    int currentQuantity = product.Quantity ?? 0;

                    int adjustmentQuantity = actualNewQuantity - currentQuantity;

                    if (adjustmentQuantity == 0)
                    {
                        transaction.Rollback(); // Không có gì thay đổi, hủy transaction
                        return;
                    }

                    // Bước 1: Cập nhật số lượng mới
                    product.Quantity = actualNewQuantity;

                    // Bước 2: Tạo lịch sử
                    var movement = new StockMovement
                    {
                        ProductId = productId,

                        // *** LỖI ĐÃ ĐƯỢC SỬA ***
                        // Bây giờ gán (int?) = (int?) nên hoàn toàn hợp lệ
                        WarehouseId = product.WarehouseId,

                        Type = "Adjust",
                        Quantity = adjustmentQuantity,
                        Date = DateTime.Now,
                        UserId = userId,
                        RelatedId = null
                    };

                    _context.StockMovements.Add(movement);

                    // Lưu cả 2 thay đổi
                    _context.SaveChanges();

                    // Hoàn tất giao dịch
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    // Nếu có lỗi, rollback tất cả
                    transaction.Rollback();
                    throw new Exception("Điều chỉnh kho thất bại. " + ex.Message);
                }
            }
        }
    }
}