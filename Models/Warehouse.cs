using System;
using System.Collections.Generic;

namespace PCShop.Models;

public partial class Warehouse
{
    public int WarehouseId { get; set; }

    public string WarehouseName { get; set; } = null!;

    public string? Location { get; set; }

    public string? Manager { get; set; }



    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    public virtual ICollection<StockEntry> StockEntries { get; set; } = new List<StockEntry>();

    public virtual ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
}
