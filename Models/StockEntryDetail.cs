using System;
using System.Collections.Generic;

namespace PCShop.Models;

public partial class StockEntryDetail
{
    public int DetailId { get; set; }

    public int? EntryId { get; set; }

    public int? ProductId { get; set; }

    public int? Quantity { get; set; }

    public decimal? UnitPrice { get; set; }

    public decimal TotalPrice => (Quantity ?? 0) * (UnitPrice ?? 0);

    public virtual StockEntry? Entry { get; set; }

    public virtual Product? Product { get; set; }
}
