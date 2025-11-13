using System;
using System.Collections.Generic;

namespace PCShop.Models;

public partial class SalesOrderDetail
{
    public int SalesOrderDetailId { get; set; }

    public int SalesId { get; set; }

    public int? ProductId { get; set; }

    public int? Quantity { get; set; }

    public decimal? UnitPrice { get; set; }

    public decimal TotalPrice => (Quantity ?? 0) * (UnitPrice ?? 0);

    public virtual Product? Product { get; set; }

    public virtual SalesOrder Sales { get; set; } = null!;
}
