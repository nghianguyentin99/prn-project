using System;
using System.Collections.Generic;

namespace PCShop.Models;

public partial class StockEntry
{
    public int EntryId { get; set; }

    public int? SupplierId { get; set; }

    public int? UserId { get; set; }

    public int? WarehouseId { get; set; }

    public DateTime? EntryDate { get; set; }

    public decimal? TotalAmount { get; set; }

    public string? Note { get; set; }

    public int Status { get; set; }

    public int? ApprovedByUserId { get; set; }

    public DateTime? ApprovedDate { get; set; }

    public virtual ICollection<StockEntryDetail> StockEntryDetails { get; set; } = new List<StockEntryDetail>();

    public virtual Supplier? Supplier { get; set; }

    public virtual User? User { get; set; }

    public virtual Warehouse? Warehouse { get; set; }
}
