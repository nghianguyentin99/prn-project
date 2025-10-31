using PCShop.Models;
using System.Collections.Generic;
using System.Linq;

namespace PCShop.Repository
{
    public class WarehouseRepository
    {
        private readonly PcshopDbContext _context;

        public WarehouseRepository()
        {
            _context = new PcshopDbContext();
        }

        public List<Warehouse> GetAll()
        {
            return _context.Warehouses.ToList();
        }
    }
}