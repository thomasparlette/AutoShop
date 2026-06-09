using AutoShop.Core.Entities;
using Microsoft.EntityFrameworkCore;

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
        }
    }
}
