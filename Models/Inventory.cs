using System;
using System.Collections.Generic;

namespace PCShop.Models;

public partial class Inventory
{
    public int InventoryId { get; set; }

    public int ProductId { get; set; }

    public int WarehouseId { get; set; }

    public int? QuantityIn { get; set; }

    public int? QuantityOut { get; set; }

    public int? CurrentStock { get; set; }

    public DateTime? LastUpdated { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual Warehouse Warehouse { get; set; } = null!;
}
