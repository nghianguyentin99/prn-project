using Microsoft.EntityFrameworkCore;
using PCShop.Models;
using System;
using System.Linq;
using System.Collections.Generic;

namespace PCShop.Repository
{
    public class DashboardRepository
    {
        private readonly PcshopDbContext _context;

        public DashboardRepository()
        {
            _context = new PcshopDbContext();
        }

        // 1. Tổng số sản phẩm đang kinh doanh
        public int GetTotalProducts()
        {
            return _context.Products.Count();
        }

        // 2. Tổng giá trị hàng tồn kho (Số lượng * Giá nhập/bán hiện tại)
        public decimal GetTotalInventoryValue()
        {
            // Lưu ý: Price trong Product là decimal?, Quantity là int?
            return _context.Products
                .Sum(p => (decimal)(p.Quantity ?? 0) * (p.Price ?? 0));
        }

        // 3. Số lượng sản phẩm sắp hết hàng (Cần nhập thêm)
        public int GetLowStockCount()
        {
            // Logic: Tồn kho thực tế <= Mức tối thiểu (MinStockLevel)
            return _context.Products
                .Where(p => p.Quantity <= p.MinStockLevel)
                .Count();
        }

        // 4. Số lượng phiếu đang chờ duyệt (Cả Nhập và Xuất)
        public int GetPendingRequestsCount()
        {
            int pendingEntries = _context.StockEntries.Count(e => e.Status == 0);
            int pendingOrders = _context.SalesOrders.Count(o => o.Status == 0);
            return pendingEntries + pendingOrders;
        }

        public List<KeyValuePair<string, int>> GetTopSellingProducts()
        {
            // Logic:
            // 1. Vào bảng chi tiết đơn hàng (SalesOrderDetails)
            // 2. Chỉ lấy các đơn hàng ĐÃ DUYỆT (SalesOrder.Status == 1)
            // 3. Group theo Tên sản phẩm
            // 4. Tính tổng số lượng bán
            // 5. Sắp xếp giảm dần và lấy 5 cái đầu tiên

            var data = _context.SalesOrderDetails
                .Include(d => d.Product)
                .Include(d => d.Sales)
                .Where(d => d.Sales.Status == 1) // Chỉ tính đơn đã duyệt
                .GroupBy(d => d.Product.Name)
                .Select(g => new
                {
                    ProductName = g.Key,
                    TotalSold = g.Sum(x => x.Quantity ?? 0)
                })
                .OrderByDescending(x => x.TotalSold)
                .Take(5)
                .ToList();

            // Chuyển đổi sang dạng KeyValuePair để dễ dùng ở View
            var result = new List<KeyValuePair<string, int>>();
            foreach (var item in data)
            {
                result.Add(new KeyValuePair<string, int>(item.ProductName, item.TotalSold));
            }

            return result;
        }
    }
}