using Microsoft.EntityFrameworkCore;
using PCShop.Models;
using System.Collections.Generic;
using System.Linq;

namespace PCShop.Repository
{
    public class StockMovementRepository
    {
        private readonly PcshopDbContext _context;

        public StockMovementRepository()
        {
            _context = new PcshopDbContext();
        }

        /// <summary>
        /// Lấy lịch sử thay đổi của một sản phẩm
        /// </summary>
        public List<StockMovement> GetHistoryByProductId(int productId)
        {
            return _context.StockMovements
                .Include(m => m.Product) // Tải thông tin Sản phẩm
                .Include(m => m.User)     // Tải thông tin Người dùng
                .Where(m => m.ProductId == productId)
                .OrderByDescending(m => m.Date) // Mới nhất lên trên
                .ToList();
        }
    }
}