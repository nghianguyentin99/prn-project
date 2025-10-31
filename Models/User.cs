using System;
using System.Collections.Generic;

namespace PCShop.Models;

public partial class User
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string? FullName { get; set; }

    public int Role { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<SalesOrder> SalesOrders { get; set; } = new List<SalesOrder>();

    public virtual ICollection<StockEntry> StockEntries { get; set; } = new List<StockEntry>();
}
