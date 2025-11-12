using System;
using System.Collections.Generic;

namespace PCShop.Models;

public partial class SalesOrder
{
    public int SalesId { get; set; }

    public int? UserId { get; set; }

    public DateTime? SaleDate { get; set; }

    public decimal? TotalAmount { get; set; }

    public string? CustomerName { get; set; }

    public string? Note { get; set; }

    public int Status { get; set; }

    public int? ApprovedByUserId { get; set; }

    public DateTime? ApprovedDate { get; set; }

    public virtual ICollection<SalesOrderDetail> SalesOrderDetails { get; set; } = new List<SalesOrderDetail>();

    public virtual User? User { get; set; }
}
