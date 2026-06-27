using AutoShop.Core.Entities;
using Microsoft.EntityFrameworkCore;
using AutoShop.Core.Enums;

namespace AutoShop.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Vehicle> Vehicles => Set<Vehicle>();
        public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
        public DbSet<WorkOrderLineItem> WorkOrderLineItems => Set<WorkOrderLineItem>();
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<Appointment> Appointments => Set<Appointment>();
        public DbSet<AppUser> Users => Set<AppUser>();
        public DbSet<ShopSettings> ShopSettings => Set<ShopSettings>();
        public DbSet<WorkOrderInspection> WorkOrderInspections => Set<WorkOrderInspection>();
        public DbSet<WorkOrderInspectionItem> WorkOrderInspectionItems => Set<WorkOrderInspectionItem>();
        public DbSet<Technician> Technicians => Set<Technician>();
        public DbSet<Part> Parts => Set<Part>();
        public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
        public DbSet<PurchaseOrderLineItem> PurchaseOrderLineItems => Set<PurchaseOrderLineItem>();
        public DbSet<InventoryTransaction> InventoryTransactions => Set<InventoryTransaction>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Customer>()
                .HasMany(c => c.Vehicles)
                .WithOne(v => v.Customer!)
                .HasForeignKey(v => v.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Customer>()
                .HasMany(c => c.WorkOrders)
                .WithOne(w => w.Customer!)
                .HasForeignKey(w => w.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Customer>()
                .HasMany(c => c.Appointments)
                .WithOne(a => a.Customer!)
                .HasForeignKey(a => a.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Vehicle>()
                .HasMany(v => v.WorkOrders)
                .WithOne(w => w.Vehicle!)
                .HasForeignKey(w => w.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Vehicle>()
                .HasMany(v => v.Appointments)
                .WithOne(a => a.Vehicle!)
                .HasForeignKey(a => a.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<WorkOrder>()
                .HasMany(w => w.LineItems)
                .WithOne(li => li.WorkOrder!)
                .HasForeignKey(li => li.WorkOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WorkOrder>()
                .HasMany(w => w.Payments)
                .WithOne(p => p.WorkOrder!)
                .HasForeignKey(p => p.WorkOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WorkOrder>()
                .HasIndex(w => w.WorkOrderNumber)
                .IsUnique();

            modelBuilder.Entity<Vehicle>()
                .HasIndex(v => v.Vin);

            modelBuilder.Entity<Customer>()
                .HasIndex(c => new { c.LastName, c.FirstName });

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.WorkOrder)
                .WithMany()
                .HasForeignKey(a => a.WorkOrderId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<WorkOrder>()
                .Property(w => w.Status)
                .HasConversion<int>();

            modelBuilder.Entity<Appointment>()
                .Property(a => a.Status)
                .HasConversion<int>();

            modelBuilder.Entity<AppUser>()
                .HasIndex(u => u.UserName)
                .IsUnique();

            modelBuilder.Entity<AppUser>()
                .Property(u => u.Role)
                .HasConversion<int>();

            modelBuilder.Entity<ShopSettings>()
                .Property(s => s.TaxRate)
                .HasPrecision(18, 2);

            modelBuilder.Entity<ShopSettings>()
                .Property(s => s.NextInvoiceNumber)
                .HasDefaultValue(1);

            modelBuilder.Entity<ShopSettings>().HasData(new ShopSettings
            {
                Id = 1,
                ShopName = "AutoShop",
                TaxRate = 0m,
                InvoicePrefix = "WO-",
                NextInvoiceNumber = 1,
                ReceiptFooterText = "Thank you for your business."
            });

            modelBuilder.Entity<WorkOrder>()
                .HasOne(w => w.Inspection)
                .WithOne(i => i.WorkOrder!)
                .HasForeignKey<WorkOrderInspection>(i => i.WorkOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WorkOrderInspection>()
                .HasMany(i => i.Items)
                .WithOne(x => x.WorkOrderInspection!)
                .HasForeignKey(x => x.WorkOrderInspectionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WorkOrderInspection>()
                .HasIndex(i => i.WorkOrderId)
                .IsUnique();

            modelBuilder.Entity<WorkOrderInspectionItem>()
                .Property(i => i.Status)
                .HasConversion<int>();
            
            modelBuilder.Entity<Technician>()
                .HasIndex(t => new { t.LastName, t.FirstName });

            modelBuilder.Entity<WorkOrder>()
                .HasOne(w => w.Technician)
                .WithMany()
                .HasForeignKey(w => w.TechnicianId)
                .OnDelete(DeleteBehavior.SetNull);
            
            modelBuilder.Entity<Part>()
                .HasIndex(p => p.PartNumber)
                .IsUnique();

            modelBuilder.Entity<Part>()
                .Property(p => p.Cost)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Part>()
                .Property(p => p.SellPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<PurchaseOrder>()
                .HasMany(p => p.LineItems)
                .WithOne(li => li.PurchaseOrder!)
                .HasForeignKey(li => li.PurchaseOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<InventoryTransaction>()
                .HasOne(t => t.Part)
                .WithMany()
                .HasForeignKey(t => t.PartId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<InventoryTransaction>()
                .Property(t => t.TransactionType)
                .HasMaxLength(50);

            modelBuilder.Entity<InventoryTransaction>()
                .Property(t => t.ReferenceNumber)
                .HasMaxLength(50);

            modelBuilder.Entity<InventoryTransaction>()
                .Property(t => t.Notes)
                .HasMaxLength(500);
            
            modelBuilder.Entity<WorkOrderLineItem>()
                .HasOne(x => x.Part)
                .WithMany()
                .HasForeignKey(x => x.PartId)
                .OnDelete(DeleteBehavior.Restrict);
        }
       
    }
}
