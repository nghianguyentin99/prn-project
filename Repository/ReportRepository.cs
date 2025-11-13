using Microsoft.EntityFrameworkCore;
using PCShop.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PCShop.Repository
{
    public class ReportRepository
    {
        private readonly PcshopDbContext _context;

        public ReportRepository()
        {
            _context = new PcshopDbContext();
        }

        // 1. Lấy lịch sử biến động kho (Nhập/Xuất/Điều chỉnh)
        public List<StockMovement> GetStockMovements(DateTime startDate, DateTime endDate, string type = "All")
        {
            var query = _context.StockMovements
                .Include(m => m.Product)
                .Include(m => m.Warehouse)
                .Include(m => m.User)
                .Where(m => m.Date >= startDate && m.Date <= endDate);

            if (type != "All")
            {
                query = query.Where(m => m.Type == type);
            }

            return query.OrderByDescending(m => m.Date).ToList();
        }

        // 2. Lấy danh sách đơn hàng đã bán (Doanh thu) - Chỉ lấy đơn ĐÃ DUYỆT (Status=1)
        public List<SalesOrder> GetSalesRevenue(DateTime startDate, DateTime endDate)
        {
            return _context.SalesOrders
                .Include(s => s.User)
                .Where(s => s.SaleDate >= startDate
                         && s.SaleDate <= endDate
                         && s.Status == 1)
                .OrderByDescending(s => s.SaleDate)
                .ToList();
        }

        // 3. Lấy danh sách phiếu nhập đã duyệt (Chi phí) - Chỉ lấy đơn ĐÃ DUYỆT (Status=1)
        public List<StockEntry> GetImportExpenditure(DateTime startDate, DateTime endDate)
        {
            return _context.StockEntries
                .Include(e => e.Supplier)
                .Include(e => e.User)
                .Where(e => e.EntryDate >= startDate
                         && e.EntryDate <= endDate
                         && e.Status == 1)
                .OrderByDescending(e => e.EntryDate)
                .ToList();
        }
    }
}