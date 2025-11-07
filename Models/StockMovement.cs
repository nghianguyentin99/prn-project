using System;
using System.Collections.Generic;

namespace PCShop.Models;

public partial class StockMovement
{
    public int MovementId { get; set; }

    public int? ProductId { get; set; }

    public int? WarehouseId { get; set; }

    public string? Type { get; set; }

    public int? Quantity { get; set; }

    public DateTime? Date { get; set; }

    public int? RelatedId { get; set; }

    public int? UserId { get; set; }

    public virtual Product? Product { get; set; }

    public virtual User? User { get; set; }

    public virtual Warehouse? Warehouse { get; set; }
}
