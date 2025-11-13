using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace PCShop.Models;

public partial class PcshopDbContext : DbContext
{
    public PcshopDbContext()
    {
    }

    public PcshopDbContext(DbContextOptions<PcshopDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<SalesOrder> SalesOrders { get; set; }

    public virtual DbSet<SalesOrderDetail> SalesOrderDetails { get; set; }

    public virtual DbSet<StockEntry> StockEntries { get; set; }

    public virtual DbSet<StockEntryDetail> StockEntryDetails { get; set; }

    public virtual DbSet<StockMovement> StockMovements { get; set; }

    public virtual DbSet<Supplier> Suppliers { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Warehouse> Warehouses { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(config.GetConnectionString("MyCnn"));
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__Categori__19093A2B0CB95B04");

            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.CategoryName).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(255);
        });

       

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PK__Products__B40CC6ED21CAA3FC");

            entity.Property(e => e.ProductId).HasColumnName("ProductID");
            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Quantity).HasDefaultValue(0);
            entity.Property(e => e.SupplierId).HasColumnName("SupplierID");
            entity.Property(e => e.Unit).HasMaxLength(20);
            entity.Property(e => e.WarehouseId).HasColumnName("WarehouseID");

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK__Products__Catego__5812160E");

            entity.HasOne(d => d.Supplier).WithMany(p => p.Products)
                .HasForeignKey(d => d.SupplierId)
                .HasConstraintName("FK__Products__Suppli__59063A47");

            entity.HasOne(d => d.Warehouse).WithMany(p => p.Products)
                .HasForeignKey(d => d.WarehouseId)
                .HasConstraintName("FK__Products__Wareho__59FA5E80");
        });

        modelBuilder.Entity<SalesOrder>(entity =>
        {
            entity.HasKey(e => e.SalesId).HasName("PK__SalesOrd__C952FB121FE423B3");

            entity.Property(e => e.SalesId).HasColumnName("SalesID");
            entity.Property(e => e.ApprovedByUserId).HasColumnName("ApprovedByUserID");
            entity.Property(e => e.ApprovedDate).HasColumnType("datetime");
            entity.Property(e => e.CustomerName).HasMaxLength(100);
            entity.Property(e => e.Note).HasMaxLength(255);
            entity.Property(e => e.SaleDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TotalAmount)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.SalesOrders)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__SalesOrde__UserI__18EBB532");
        });

        modelBuilder.Entity<SalesOrderDetail>(entity =>
        {
            entity.HasKey(e => e.SalesOrderDetailId).HasName("PK__SalesOrd__6B9B5125F63B046A");

            entity.Property(e => e.ProductId).HasColumnName("ProductID");
            entity.Property(e => e.SalesId).HasColumnName("SalesID");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Product).WithMany(p => p.SalesOrderDetails)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK__SalesOrde__Produ__1CBC4616");

            entity.HasOne(d => d.Sales).WithMany(p => p.SalesOrderDetails)
                .HasForeignKey(d => d.SalesId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SalesOrde__Sales__1BC821DD");
        });

        modelBuilder.Entity<StockEntry>(entity =>
        {
            entity.HasKey(e => e.EntryId).HasName("PK__StockEnt__F57BD2D727787163");

            entity.Property(e => e.EntryId).HasColumnName("EntryID");
            entity.Property(e => e.ApprovedByUserId).HasColumnName("ApprovedByUserID");
            entity.Property(e => e.ApprovedDate).HasColumnType("datetime");
            entity.Property(e => e.EntryDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Note).HasMaxLength(255);
            entity.Property(e => e.SupplierId).HasColumnName("SupplierID");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.WarehouseId).HasColumnName("WarehouseID");

            entity.HasOne(d => d.Supplier).WithMany(p => p.StockEntries)
                .HasForeignKey(d => d.SupplierId)
                .HasConstraintName("FK__StockEntr__Suppl__6477ECF3");

            entity.HasOne(d => d.User).WithMany(p => p.StockEntries)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__StockEntr__UserI__656C112C");

            entity.HasOne(d => d.Warehouse).WithMany(p => p.StockEntries)
                .HasForeignKey(d => d.WarehouseId)
                .HasConstraintName("FK__StockEntr__Wareh__66603565");
        });

        modelBuilder.Entity<StockEntryDetail>(entity =>
        {
            entity.HasKey(e => e.DetailId).HasName("PK__StockEnt__135C314D9A302F4C");

            entity.Property(e => e.DetailId).HasColumnName("DetailID");
            entity.Property(e => e.EntryId).HasColumnName("EntryID");
            entity.Property(e => e.ProductId).HasColumnName("ProductID");
           
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Entry).WithMany(p => p.StockEntryDetails)
                .HasForeignKey(d => d.EntryId)
                .HasConstraintName("FK__StockEntr__Entry__693CA210");

            entity.HasOne(d => d.Product).WithMany(p => p.StockEntryDetails)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK__StockEntr__Produ__6A30C649");
        });

        modelBuilder.Entity<StockMovement>(entity =>
        {
            entity.HasKey(e => e.MovementId).HasName("PK__StockMov__D18224661F78422D");

            entity.Property(e => e.MovementId).HasColumnName("MovementID");
            entity.Property(e => e.Date)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ProductId).HasColumnName("ProductID");
            entity.Property(e => e.RelatedId).HasColumnName("RelatedID");
            entity.Property(e => e.Type).HasMaxLength(10);
            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.WarehouseId).HasColumnName("WarehouseID");

            entity.HasOne(d => d.Product).WithMany(p => p.StockMovements)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK__StockMove__Produ__73BA3083");

            entity.HasOne(d => d.User).WithMany(p => p.StockMovements)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__StockMove__UserI__75A278F5");

            entity.HasOne(d => d.Warehouse).WithMany(p => p.StockMovements)
                .HasForeignKey(d => d.WarehouseId)
                .HasConstraintName("FK__StockMove__Wareh__74AE54BC");
        });

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasKey(e => e.SupplierId).HasName("PK__Supplier__4BE666940EE4CC6E");

            entity.Property(e => e.SupplierId).HasColumnName("SupplierID");
            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(20);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CCAC948327B3");

            entity.HasIndex(e => e.Username, "UQ__Users__536C85E43B51F820").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Password).HasMaxLength(255);
            entity.Property(e => e.Role).HasDefaultValue(1);
            entity.Property(e => e.Username).HasMaxLength(50);
        });

        modelBuilder.Entity<Warehouse>(entity =>
        {
            entity.HasKey(e => e.WarehouseId).HasName("PK__Warehous__2608AFD9E943E66D");

            entity.Property(e => e.WarehouseId).HasColumnName("WarehouseID");
            entity.Property(e => e.Location).HasMaxLength(255);
            entity.Property(e => e.Manager).HasMaxLength(100);
            entity.Property(e => e.WarehouseName).HasMaxLength(100);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
