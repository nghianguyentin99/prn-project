using System;
using System.Collections.Generic;

namespace PCShop.Models;

public partial class SalesOrder
{
    public int SalesId { get; set; }

    public int? UserId { get; set; }

    public int? ProductId { get; set; }

    public int? Quantity { get; set; }

    public decimal? UnitPrice { get; set; }

    public DateTime? SaleDate { get; set; }

    public int Status { get; set; }

    public int? ApprovedByUserId { get; set; }

    public DateTime? ApprovedDate { get; set; }

    public virtual Product? Product { get; set; }

    public virtual User? User { get; set; }
}
