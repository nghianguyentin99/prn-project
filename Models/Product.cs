using System;
using System.Collections.Generic;

namespace PCShop.Models;

public partial class Product
{
    public int ProductId { get; set; }

    public string Name { get; set; } = null!;

    public int? CategoryId { get; set; }

    public int? SupplierId { get; set; }

    public int? WarehouseId { get; set; }

    public int? Quantity { get; set; }

    public string? Unit { get; set; }

    public decimal? Price { get; set; }

    public string? Description { get; set; }

    public int MinStockLevel { get; set; }

    public virtual Category? Category { get; set; }


    public virtual ICollection<SalesOrderDetail> SalesOrderDetails { get; set; } = new List<SalesOrderDetail>();

    public virtual ICollection<StockEntryDetail> StockEntryDetails { get; set; } = new List<StockEntryDetail>();

    public virtual ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();

    public virtual Supplier? Supplier { get; set; }

    public virtual Warehouse? Warehouse { get; set; }
}
