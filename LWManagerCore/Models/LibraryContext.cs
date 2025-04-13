using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace LWManagerCore.Models
{
    internal class LibraryContext :DbContext
    {
        public DbSet<LeaseContract> LeaseContracts { get; set; } = null!;
        public DbSet<ArchiveLeaseContract> ArchiveLeaseContracts { get; set; }
        public DbSet<ReturnedLeaseContract> ReturnedLeaseContracts { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductReference> ProductReferences { get; set; }
        public DbSet<OrderProduct> OrderProducts { get; set; }
        public DbSet<ReturnedProduct> ReturnedProducts { get; set; }
        public DbSet<Payment> Payments { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySQL($"server={Properties.Settings.Default.MysqlBackupUrl};" +
                $"database={Properties.Settings.Default.MysqlDatabaseName};" +
                $"user={Properties.Settings.Default.MysqlUsername};" +
                //$"password={MainUtils._SystemTracert(Properties.Settings.Default.MysqlPassword)}");
                $"password=#bWO9P*el861lD");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<LeaseContract>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Client_id).IsRequired();
                entity.Property(e => e.Contract_id).IsRequired();
                entity.Property(e => e.Paid_amount).IsRequired();
                entity.Property(e => e.Price_per_day).IsRequired();
                entity.Property(e => e.Delivery_amount).IsRequired();
                entity.Property(e => e.Delivery_address);
                entity.Property(e => e.Used_days).IsRequired();
                entity.Property(e => e.Create_datetime).IsRequired();
                entity.Property(e => e.Return_datetime).IsRequired();
                entity.Property(e => e.Close_datetime).IsRequired();
                entity.Property(e => e.Note);
            });

            modelBuilder.Entity<ArchiveLeaseContract>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Order_id).IsRequired();
                entity.Property(e => e.Client_id).IsRequired();
                entity.Property(e => e.Contract_id).IsRequired();
                entity.Property(e => e.Paid_amount).IsRequired();
                entity.Property(e => e.Price_per_day).IsRequired();
                entity.Property(e => e.Delivery_amount).IsRequired();
                entity.Property(e => e.Delivery_address);
                entity.Property(e => e.Used_days).IsRequired();
                entity.Property(e => e.Create_datetime).IsRequired();
                entity.Property(e => e.Return_datetime).IsRequired();
                entity.Property(e => e.Close_datetime).IsRequired();
                entity.Property(e => e.Note);
            });

            modelBuilder.Entity<ReturnedLeaseContract>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Order_id).IsRequired();
                entity.Property(e => e.Client_id).IsRequired();
                entity.Property(e => e.Contract_id).IsRequired();
                entity.Property(e => e.Paid_amount).IsRequired();
                entity.Property(e => e.Price_per_day).IsRequired();
                entity.Property(e => e.Delivery_amount).IsRequired();
                entity.Property(e => e.Delivery_address);
                entity.Property(e => e.Used_days).IsRequired();
                entity.Property(e => e.Create_datetime).IsRequired();
                entity.Property(e => e.Return_datetime).IsRequired();
                entity.Property(e => e.Close_datetime).IsRequired();
                entity.Property(e => e.Note);
            });

            modelBuilder.Entity<Client>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name);
                entity.Property(e => e.Surname);
                entity.Property(e => e.Middle_name);
                entity.Property(e => e.Pass_number);
                entity.Property(e => e.Phone_number);
                entity.Property(e => e.Phone_number2);
                entity.Property(e => e.Address);
                entity.Property(e => e.Last_order_datetime).IsRequired();
                entity.Property(e => e.Is_blocked).IsRequired();
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name);
                entity.Property(e => e.Price).IsRequired();
                entity.Property(e => e.Count).IsRequired();
                entity.Property(e => e.Reference_id).IsRequired();
            });

            modelBuilder.Entity<ProductReference>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Product_id).IsRequired();
                entity.Property(e => e.Ref_product_id).IsRequired();
                entity.Property(e => e.Ref_product_count).IsRequired();
            });

            modelBuilder.Entity<OrderProduct>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Order_id).IsRequired();
                entity.Property(e => e.Product_id).IsRequired();
                entity.Property(e => e.Count).IsRequired();
                entity.Property(e => e.Price).IsRequired();
            });

            modelBuilder.Entity<ReturnedProduct>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Order_id).IsRequired();
                entity.Property(e => e.Product_id).IsRequired();
                entity.Property(e => e.Count).IsRequired();
                entity.Property(e => e.Price).IsRequired();
            });

            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Amount).IsRequired();
                entity.Property(e => e.Datetime).IsRequired();
                entity.Property(e => e.Order_id).IsRequired();
                entity.Property(e => e.Payment_type).IsRequired();
            });
        }
    }
}
